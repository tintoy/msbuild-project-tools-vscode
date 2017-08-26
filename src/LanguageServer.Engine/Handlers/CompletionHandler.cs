using JsonRpc;
using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Models;
using Lsp.Protocol;
using Microsoft.Language.Xml;
using NuGet.Versioning;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
    
namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using ContentProviders;
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

            List<CompletionItem> completionItems = new List<CompletionItem>();
            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                if (!projectDocument.HasXml)
                    return null;

                Position position = parameters.Position.ToNative();

                // Try to match up the position with an element or attribute in the XML.
                SyntaxNode xml = projectDocument.GetXmlAtPosition(position);
                if (xml == null)
                    return null;

                // Are we on an attribute?
                XmlAttributeSyntax attribute = xml.GetContainingAttribute();
                if (attribute == null)
                    return null;

                // Must be a PackageReference element.
                if (!String.Equals(attribute.ParentElement.Name, "PackageReference", StringComparison.OrdinalIgnoreCase))
                    return null;

                // Are we on the attribute's value?
                Range attributeValueRange = attribute.GetValueRange(projectDocument.XmlPositions);
                if (!attributeValueRange.Contains(position))
                    return null;

                try
                {
                    if (attribute.Name == "Include")
                    {
                        string packageIdPrefix = attribute.Value;
                        SortedSet<string> packageIds = await projectDocument.SuggestPackageIds(packageIdPrefix, cancellationToken);
                        
                        completionItems.AddRange(
                            packageIds.Select(packageId => new CompletionItem
                            {
                                Label = packageId,
                                Kind = CompletionItemKind.Module,
                                TextEdit = new TextEdit
                                {
                                    Range = attributeValueRange.ToLsp(),
                                    NewText = packageId
                                }
                            })
                        );
                    }
                    else if (attribute.Name == "Version")
                    {
                        XmlAttributeSyntax includeAttribute = attribute.ParentElement.AsSyntaxElement["Include"];
                        if (includeAttribute == null)
                            return null;

                        string packageId = includeAttribute.Value;
                        IEnumerable<NuGetVersion> packageVersions = await projectDocument.SuggestPackageVersions(packageId, cancellationToken);
                        if (Workspace.Configuration.ShowNewestNuGetVersionsFirst)
                            packageVersions = packageVersions.Reverse();
                            
                        Lsp.Models.Range replacementRange = attributeValueRange.ToLsp();
                        
                        completionItems.AddRange(
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
                    }
                    else
                        return null; // No completions.
                }
                catch (AggregateException aggregateSuggestionError)
                {
                    foreach (Exception suggestionError in aggregateSuggestionError.Flatten().InnerExceptions)
                    {
                        Log.Error(suggestionError, "Failed to provide completions.");
                    }
                }
                catch (Exception suggestionError)
                {
                    Log.Error(suggestionError, "Failed to provide completions.");
                }
            }

            CompletionList completionList = new CompletionList(completionItems,
                isIncomplete: completionItems.Count >= 20 // Default page size.
            );

            return completionList;
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
