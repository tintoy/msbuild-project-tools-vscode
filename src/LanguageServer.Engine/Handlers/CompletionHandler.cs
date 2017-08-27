using JsonRpc;
using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Models;
using Lsp.Protocol;
using NuGet.Versioning;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using Documents;
    using SemanticModel;
    using Utilities;

    /// <summary>
    ///     Handler for completion requests.
    /// </summary>
    public sealed class CompletionHandler
        : Handler, ICompletionHandler
    {
        /// <summary>
        ///     Create a new <see cref="CompletionHandler"/>.
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
        public CompletionHandler(ILanguageServer server, Workspace workspace, ILogger logger)
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
        ///     The language server configuration.
        /// </summary>
        Configuration Configuration { get; }

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
        ///     Get registration options for handling completion requests events.
        /// </summary>
        CompletionRegistrationOptions CompletionRegistrationOptions
        {
            get => new CompletionRegistrationOptions
            {
                DocumentSelector = DocumentSelector,
                ResolveProvider = false
            };
        }

        /// <summary>
        ///     The server's completion capabilities.
        /// </summary>
        CompletionCapability CompletionCapabilities { get; set; }

        /// <summary>
        ///     Called when completions are requested.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the completion list or <c>null</c> if no completions are provided.
        /// </returns>
        async Task<CompletionList> OnCompletion(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            ProjectDocument projectDocument = await Workspace.GetProjectDocument(parameters.TextDocument.Uri);

            List<CompletionItem> completionItems = null;
            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                if (!projectDocument.HasXml)
                    return null;

                Position position = parameters.Position.ToNative();
                XmlLocation location = projectDocument.XmlLocator.Inspect(position);
                if (location == null)
                    return null;

                Log.Information("Completion requested for {Valid:l} {NodeKind} @ {NodeRange:l}/{NodeLength} ({LocationFlags})",
                    location.Node.IsValid ? "valid" : "invalid",
                    location.Node.Kind,
                    location.Node.Range,
                    projectDocument.XmlPositions.GetLength(location.Node.Range),
                    location.Flags
                );
                XSElement replaceElement;
                if (location.CanCompleteElement(out replaceElement))
                {
                    Log.Information("Completion handler would be able to replace element @ {Range:l}", replaceElement.Range);
                }

                XSAttribute attribute;
                if (!location.IsAttribute(out attribute))
                    return null;

                // Are we on the attribute's value?
                if (!location.IsAttributeValue())
                    return null;

                // Must be a PackageReference element.
                if (attribute.Element.Name == "PackageReference")
                    completionItems = await HandlePackageReferenceCompletion(projectDocument, attribute, cancellationToken);
            }
            if (completionItems == null)
                return null;

            CompletionList completionList = new CompletionList(completionItems,
                isIncomplete: completionItems.Count >= 10 // Default page size.
            );

            return completionList;
        }

        /// <summary>
        ///     Handle completion for an attribute of a PackageReference element.
        /// </summary>
        /// <param name="projectDocument">
        ///     The current project document.
        /// </param>
        /// <param name="attribute">
        ///     The attribute for which completion is being requested.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        async Task<List<CompletionItem>> HandlePackageReferenceCompletion(ProjectDocument projectDocument, XSAttribute attribute, CancellationToken cancellationToken)
        {
            try
            {
                if (attribute.Name == "Include")
                {
                    string packageIdPrefix = attribute.Value;
                    SortedSet<string> packageIds = await projectDocument.SuggestPackageIds(packageIdPrefix, cancellationToken);

                    var completionItems = new List<CompletionItem>(
                        packageIds.Select(packageId => new CompletionItem
                        {
                            Label = packageId,
                            Kind = CompletionItemKind.Module,
                            TextEdit = new TextEdit
                            {
                                Range = attribute.ValueRange.ToLsp(),
                                NewText = packageId
                            }
                        })
                    );
                    
                    return completionItems;
                }
                
                if (attribute.Name == "Version")
                {
                    XSAttribute includeAttribute = attribute.Element["Include"];
                    if (includeAttribute == null)
                        return null;

                    string packageId = includeAttribute.Value;
                    IEnumerable<NuGetVersion> packageVersions = await projectDocument.SuggestPackageVersions(packageId, cancellationToken);
                    if (Workspace.Configuration.ShowNewestNuGetVersionsFirst)
                        packageVersions = packageVersions.Reverse();

                    Lsp.Models.Range replacementRange = attribute.ValueRange.ToLsp();

                    var completionItems = new List<CompletionItem>(
                        packageVersions.Select((packageVersion, index) => new CompletionItem
                        {
                            Label = packageVersion.ToNormalizedString(),
                            SortText = Workspace.Configuration.ShowNewestNuGetVersionsFirst ? $"NuGet{index:00}" : null, // Override default sort order if configured to do so.
                                Kind = CompletionItemKind.Field,
                            TextEdit = new TextEdit
                            {
                                Range = replacementRange,
                                NewText = packageVersion.ToNormalizedString()
                            }
                        })
                    );

                    return completionItems;
                }

                // No completions.
                return null;
            }
            catch (AggregateException aggregateSuggestionError)
            {
                foreach (Exception suggestionError in aggregateSuggestionError.Flatten().InnerExceptions)
                    Log.Error(suggestionError, "Failed to provide completions.");
                
                return null;
            }
            catch (Exception suggestionError)
            {
                Log.Error(suggestionError, "Failed to provide completions.");

                return null;
            }
        }

        /// <summary>
        ///     Handle a request for completion.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the completion list or <c>null</c> if no completions are provided.
        /// </returns>
        async Task<CompletionList> IRequestHandler<TextDocumentPositionParams, CompletionList>.Handle(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            try
            {
                return await OnCompletion(parameters, cancellationToken);
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unhandled exception in {Method:l}.", "OnCompletion");

                return null;
            }
        }

        /// <summary>
        ///     Get registration options for handling completion requests.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        CompletionRegistrationOptions IRegistration<CompletionRegistrationOptions>.GetRegistrationOptions() => CompletionRegistrationOptions;

        /// <summary>
        ///     Called to inform the handler of the language server's completion capabilities.
        /// </summary>
        /// <param name="capabilities">
        ///     A <see cref="CompletionCapability"/> data structure representing the capabilities.
        /// </param>
        void ICapability<CompletionCapability>.SetCapability(CompletionCapability capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            CompletionCapabilities = capabilities;
        }
    }
}
