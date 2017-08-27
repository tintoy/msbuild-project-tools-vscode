using JsonRpc;
using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Models;
using Lsp.Protocol;
using Microsoft.Language.Xml;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using ContentProviders;
    using Documents;
    using SemanticModel;
    using Utilities;

    /// <summary>
    ///     Handler for document hover requests.
    /// </summary>
    public sealed class HoverHandler
        : Handler, IHoverHandler
    {
        /// <summary>
        ///     Create a new <see cref="HoverHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="workspace">
        ///     The document workspace.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public HoverHandler(ILanguageServer server, Workspace workspace, ILogger logger)
            : base(server, logger)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            Workspace = workspace;
        }

        /// <summary>
        ///     The document workspace.
        /// </summary>
        Workspace Workspace { get; }

        /// <summary>
        ///     The document selector that describes documents to synchronise.
        /// </summary>
        DocumentSelector DocumentSelector { get; } = new DocumentSelector(
            new DocumentFilter
            {
                Pattern = "**/*.*",
                Language = "msbuild"
            },
            new DocumentFilter
            {
                Pattern = "**/*.*proj",
                Language = "xml"
            },
            new DocumentFilter
            {
                Pattern = "**/*.props",
                Language = "xml"
            },
            new DocumentFilter
            {
                Pattern = "**/*.targets",
                Language = "xml"
            }
        );

        /// <summary>
        ///     Get registration options for handling document events.
        /// </summary>
        TextDocumentRegistrationOptions DocumentRegistrationOptions
        {
            get => new TextDocumentRegistrationOptions
            {
                DocumentSelector = DocumentSelector
            };
        }

        /// <summary>
        ///     The server's hover capabilities.
        /// </summary>
        HoverCapability HoverCapabilities { get; set; }

        /// <summary>
        ///     Called when the mouse pointer hovers over text.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> whose result is the hover details, or <c>null</c> if no hover details are provided by the handler.
        /// </returns>
        async Task<Hover> OnHover(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            if (Workspace.Configuration.DisableHover)
                return null;

            ProjectDocument projectDocument = await Workspace.GetProjectDocument(parameters.TextDocument.Uri);

            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                // This won't work if we can't inspect the MSBuild project state.
                if (!projectDocument.HasMSBuildProject)
                    return null;

                Position position = parameters.Position.ToNative();

                XmlPosition xmlPosition = projectDocument.XmlLocator.Inspect(position);
                if (xmlPosition == null)
                    return null;

                if (!xmlPosition.IsElement && !xmlPosition.IsAttribute)
                    return null;

                // Match up the MSBuild item / property with its corresponding XML element / attribute.
                MSBuildObject msbuildObject = projectDocument.GetMSBuildObjectAtPosition(xmlPosition.Node.Start);

                MarkedStringContainer hoverContent = null;
                HoverContentProvider contentProvider = new HoverContentProvider(projectDocument);
                if (xmlPosition.IsElement && !xmlPosition.IsElementContent)
                {
                    XSElement element = (XSElement)xmlPosition.Node;

                    if (msbuildObject is MSBuildProperty propertyFromElement)
                        hoverContent = contentProvider.Property(propertyFromElement);
                    else if (msbuildObject is MSBuildUnusedProperty unusedPropertyFromElement)
                        hoverContent = contentProvider.UnusedProperty(unusedPropertyFromElement);
                    else if (msbuildObject is MSBuildItemGroup itemGroupFromElement)
                        hoverContent = contentProvider.ItemGroup(itemGroupFromElement);
                    else if (msbuildObject is MSBuildUnusedItemGroup unusedItemGroupFromElement)
                        hoverContent = contentProvider.UnusedItemGroup(unusedItemGroupFromElement);
                    else if (msbuildObject is MSBuildTarget targetFromElement)
                        hoverContent = contentProvider.Target(targetFromElement);
                    else if (msbuildObject is MSBuildImport importFromElement)
                        hoverContent = contentProvider.Import(importFromElement);
                    else if (msbuildObject is MSBuildUnresolvedImport unresolvedImportFromElement)
                        hoverContent = contentProvider.UnresolvedImport(unresolvedImportFromElement);
                }
                else if (xmlPosition.IsAttribute)
                {
                    XSAttribute attribute = (XSAttribute)xmlPosition.Node;

                    if (msbuildObject is MSBuildItemGroup itemGroupFromAttribute)
                        hoverContent = contentProvider.ItemGroupMetadata(itemGroupFromAttribute, attribute.Name);
                    else if (msbuildObject is MSBuildUnusedItemGroup unusedItemGroupFromAttribute)
                        hoverContent = contentProvider.UnusedItemGroupMetadata(unusedItemGroupFromAttribute, attribute.Name);
                    else if (msbuildObject is MSBuildSdkImport sdkImportFromAttribute)
                        hoverContent = contentProvider.SdkImport(sdkImportFromAttribute);
                    else if (msbuildObject is MSBuildUnresolvedSdkImport unresolvedSdkImportFromAttribute)
                        hoverContent = contentProvider.UnresolvedSdkImport(unresolvedSdkImportFromAttribute);
                    else if (msbuildObject is MSBuildImport importFromAttribute)
                        hoverContent = contentProvider.Import(importFromAttribute);
                    else if (attribute.Name == "Condition")
                        hoverContent = contentProvider.Condition(attribute.Element.Name, attribute.Value);
                }

                if (hoverContent == null)
                    return null;

                return new Hover
                {
                    Contents = hoverContent,
                    Range = xmlPosition.Node.Range.ToLsp()
                };
            }
        }

        /// <summary>
        ///     Get registration options for handling document events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions() => DocumentRegistrationOptions;

        /// <summary>
        ///     Handle a request for hover information.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the hover details or <c>null</c> if no hover details are provided.
        /// </returns>
        async Task<Hover> IRequestHandler<TextDocumentPositionParams, Hover>.Handle(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            try
            {
                return await OnHover(parameters, cancellationToken);
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unhandled exception in {Method:l}.", "OnHover");

                return null;
            }
        }

        /// <summary>
        ///     Called to inform the handler of the language server's hover capabilities.
        /// </summary>
        /// <param name="capabilities">
        ///     A <see cref="HoverCapability"/> data structure representing the capabilities.
        /// </param>
        void ICapability<HoverCapability>.SetCapability(HoverCapability capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            HoverCapabilities = capabilities;
        }
    }
}
