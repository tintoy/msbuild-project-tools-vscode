using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.Configuration;
using NuGet.Versioning;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using ContentProviders;
    using Documents;
    using MSBuild;
    using Utilities;

    /// <summary>
    ///     Handler for project file document events.
    /// </summary>
    public class ProjectDocumentHandler
        : TextDocumentHandler
    {
        /// <summary>
        ///     Create a new <see cref="ProjectDocumentHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="workspace">
        ///     The document workspace.
        /// </param>
        /// <param name="configuration">
        ///     The language server configuration.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public ProjectDocumentHandler(ILanguageServer server, Workspace workspace, Configuration configuration, ILogger logger)
            : base(server, logger)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            Workspace = workspace;
            Configuration = configuration;
        }

        /// <summary>
        ///     The document selector that describes documents targeted by the handler.
        /// </summary>
        protected override DocumentSelector DocumentSelector => new DocumentSelector(
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
        ///     The document workspace.
        /// </summary>
        Workspace Workspace { get; }

        /// <summary>
        ///     The language server configuration.
        /// </summary>
        Configuration Configuration { get; }

        /// <summary>
        ///     Get attributes for the specified text document.
        /// </summary>
        /// <param name="documentUri">
        ///     The document URI.
        /// </param>
        /// <returns>
        ///     The document attributes.
        /// </returns>
        protected override TextDocumentAttributes GetTextDocumentAttributes(Uri documentUri)
        {
            string documentFilePath = VSCodeDocumentUri.GetFileSystemPath(documentUri);
            if (documentFilePath == null)
                return base.GetTextDocumentAttributes(documentUri);

            string extension = Path.GetExtension(documentFilePath).ToLower();
            switch (extension)
            {
                case "props":
                case "targets":
                {
                    break;
                }
                default:
                {
                    if (extension.EndsWith("proj"))
                        break;

                    return base.GetTextDocumentAttributes(documentUri);
                }
            }

            return new TextDocumentAttributes(documentUri, "msbuild");
        }

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
        protected override async Task<Hover> OnHover(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            if (Configuration.DisableHover)
                return null;

            ProjectDocument projectDocument = await Workspace.GetProjectDocument(parameters.TextDocument.Uri);
            
            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                // This won't work if we can't inspect the MSBuild project state.
                if (!projectDocument.HasMSBuildProject)
                    return null;

                Position position = parameters.Position.ToNative();

                // Try to match up the position with an element or attribute in the XML, then match that up with an MSBuild object.
                SyntaxNode xmlNode = projectDocument.GetXmlAtPosition(position);
                if (xmlNode == null)
                    return null;

                SyntaxNode elementOrAttribute = xmlNode.GetContainingElementOrAttribute();
                if (elementOrAttribute == null)
                    return null;

                position = elementOrAttribute.Span.ToNative(projectDocument.XmlPositions).Start;

                // Match up the MSBuild item / property with its corresponding XML element / attribute.
                MSBuildObject msbuildObject = projectDocument.GetMSBuildObjectAtPosition(position);
                if (msbuildObject == null)
                    return null;

                Range range = elementOrAttribute.Span.ToNative(projectDocument.XmlPositions);
                Hover result = new Hover
                {
                    Range = range.ToLsp()
                };
                
                HoverContentProvider contentProvider = new HoverContentProvider(projectDocument);
                if (elementOrAttribute is IXmlElementSyntax element)
                {
                    if (msbuildObject is MSBuildProperty propertyFromElement)
                        result.Contents = contentProvider.Property(propertyFromElement);
                    else if (msbuildObject is MSBuildUnusedProperty unusedPropertyFromElement)
                        result.Contents = contentProvider.UnusedProperty(unusedPropertyFromElement);
                    else if (msbuildObject is MSBuildItemGroup itemGroupFromElement)
                        result.Contents = contentProvider.ItemGroup(itemGroupFromElement);
                    else if (msbuildObject is MSBuildUnusedItemGroup unusedItemGroupFromElement)
                        result.Contents = contentProvider.UnusedItemGroup(unusedItemGroupFromElement);
                    else if (msbuildObject is MSBuildTarget targetFromElement)
                        result.Contents = contentProvider.Target(targetFromElement);
                    else if (msbuildObject is MSBuildImport importFromElement)
                        result.Contents = contentProvider.Import(importFromElement);
                    else if (msbuildObject is MSBuildUnresolvedImport unresolvedImportFromElement)
                        result.Contents = contentProvider.UnresolvedImport(unresolvedImportFromElement);
                    else
                        return null;
                }
                else if (elementOrAttribute is XmlAttributeSyntax attribute)
                {
                    if (msbuildObject is MSBuildItemGroup itemGroupFromAttribute)
                        result.Contents = contentProvider.ItemGroupMetadata(itemGroupFromAttribute, attribute.Name);
                    else if (msbuildObject is MSBuildUnusedItemGroup unusedItemGroupFromAttribute)
                        result.Contents = contentProvider.UnusedItemGroupMetadata(unusedItemGroupFromAttribute, attribute.Name);
                    else if (msbuildObject is MSBuildSdkImport sdkImportFromAttribute)
                        result.Contents = contentProvider.SdkImport(sdkImportFromAttribute);
                    else if (msbuildObject is MSBuildUnresolvedSdkImport unresolvedSdkImportFromAttribute)
                        result.Contents = contentProvider.UnresolvedSdkImport(unresolvedSdkImportFromAttribute);
                    else if (msbuildObject is MSBuildImport importFromAttribute)
                        result.Contents = contentProvider.Import(importFromAttribute);
                    else
                        return null;
                }
                else
                    return null;

                return result;
            }
        }

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
        protected override async Task<CompletionList> OnCompletion(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
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
                        SortedSet<NuGetVersion> packageVersions = await projectDocument.SuggestPackageVersions(packageId, cancellationToken);
                        
                        completionItems.AddRange(
                            packageVersions.Select(packageVersion => new CompletionItem
                            {
                                Label = packageVersion.ToNormalizedString(),
                                Kind = CompletionItemKind.Field,
                                TextEdit = new TextEdit
                                {
                                    Range = attributeValueRange.ToLsp(),
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
        protected override async Task<SymbolInformationContainer> OnDocumentSymbols(DocumentSymbolParams parameters, CancellationToken cancellationToken)
        {
            ProjectDocument projectDocument = await Workspace.GetProjectDocument(parameters.TextDocument.Uri);

            List<SymbolInformation> symbols = new List<SymbolInformation>();
            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                if (!projectDocument.HasMSBuildProject)
                    return null;

                foreach (MSBuildObject msbuildObject in projectDocument.MSBuildLookup.AllObjects)
                {
                    // Special case for item groups, which can contribute multiple symbols from a single item group.
                    if (msbuildObject is MSBuildItemGroup itemGroup)
                    {
                        symbols.AddRange(itemGroup.Includes.Select(include =>
                        {
                            string trimmedInclude = String.Join(";",
                                include.Split(
                                    new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries
                                )
                                .Select(includedItem => includedItem.Trim())
                            );
                                

                            return new SymbolInformation
                            {
                                Name = $"{itemGroup.Name} ({trimmedInclude})",
                                Kind = SymbolKind.Array,
                                ContainerName = "Item",
                                Location = new Location
                                {
                                    Uri = projectDocument.DocumentUri,
                                    Range = msbuildObject.XmlRange.ToLsp()
                                }
                            };
                        }));

                        continue;
                    }

                    SymbolInformation symbol = new SymbolInformation
                    {
                        Name = msbuildObject.Name,
                        Location = new Location
                        {
                            Uri = projectDocument.DocumentUri,
                            Range = msbuildObject.XmlRange.ToLsp()
                        }
                    };
                    if (msbuildObject is MSBuildTarget)
                    {
                        symbol.ContainerName = "Target";
                        symbol.Kind = SymbolKind.Function;
                    }
                    else if (msbuildObject is MSBuildProperty)
                    {
                        symbol.ContainerName = "Property";
                        symbol.Kind = SymbolKind.Property;
                    }
                    else if (msbuildObject is MSBuildImport)
                    {
                        symbol.ContainerName = "Import";
                        symbol.Kind = SymbolKind.Package;
                    }
                    else if (msbuildObject is MSBuildSdkImport)
                    {
                        symbol.ContainerName = "Import (SDK)";
                        symbol.Kind = SymbolKind.Package;
                    }
                    else
                        continue;

                    symbols.Add(symbol);
                }
            }

            if (symbols.Count == 0)
                return null;

            return new SymbolInformationContainer(
                symbols.OrderBy(symbol => symbol.Name)
            );
        }

        /// <summary>
        ///     Called when a definition is requested.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the definition location or <c>null</c> if no definition is provided.
        /// </returns>
        protected override async Task<LocationOrLocations> OnDefinition(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            ProjectDocument projectDocument = await Workspace.GetProjectDocument(parameters.TextDocument.Uri);

            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                if (!projectDocument.HasMSBuildProject)
                    return null;

                Position position = parameters.Position.ToNative();
                MSBuildObject msbuildObjectAtPosition = projectDocument.GetMSBuildObjectAtPosition(position);
                if (msbuildObjectAtPosition == null)
                    return null;

                if (msbuildObjectAtPosition is MSBuildSdkImport import)
                {
                    if (msbuildObjectAtPosition is MSBuildSdkImport sdkImportAtPosition)
                    {
                        // TODO: Parse imported project and determine location of root element (use that range instead).
                        Location[] locations =
                            sdkImportAtPosition.ImportedProjectRoots.Select(
                                importedProjectRoot => new Location
                                {
                                    Range = Range.Empty.ToLsp(),
                                    Uri = VSCodeDocumentUri.CreateFromFileSystemPath(importedProjectRoot.Location.File)
                                }
                            )
                            .ToArray();

                        return new LocationOrLocations(locations);
                    }
                    else if (msbuildObjectAtPosition is MSBuildImport importAtPosition)
                    {
                        // TODO: Parse imported project and determine location of root element (use that range instead).
                        return new LocationOrLocations(
                            importAtPosition.ImportedProjectRoots.Select(
                                importedProjectRoot => new Location
                            {
                                Range = Range.Empty.ToLsp(),
                                Uri = VSCodeDocumentUri.CreateFromFileSystemPath(importedProjectRoot.Location.File)
                            }
                        ));
                    }
                }
            }

            return null;
        }
    }
}
