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
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using System.Text;
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
        ///     Documents for loaded project, keyed by document URI.
        /// </summary>
        readonly ConcurrentDictionary<Uri, ProjectDocument> _projectDocuments = new ConcurrentDictionary<Uri, ProjectDocument>();

        /// <summary>
        ///     Create a new <see cref="ProjectDocumentHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        /// <param name="projectDocuments"/>
        ///     Documents for loaded project files.
        /// </param>
        public ProjectDocumentHandler(ILanguageServer server, ILogger logger)
            : base(server, logger)
        {
            Options.Change = TextDocumentSyncKind.Full;
        }

        /// <summary>
        ///     The document selector that describes documents targeted by the handler.
        /// </summary>
        protected override DocumentSelector DocumentSelector => new DocumentSelector(
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
        ///     Called when a text document is opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override async Task OnDidOpenTextDocument(DidOpenTextDocumentParams parameters)
        {
            ProjectDocument projectDocument = await GetProjectDocument(parameters.TextDocument.Uri);
            PublishDiagnostics(projectDocument);

            if (!projectDocument.HasXml)
            {
                Log.Warning("Failed to load project file {ProjectFilePath}.", projectDocument.ProjectFile.FullName);

                return;
            }

            Log.Information("Successfully loaded project {ProjectFilePath}.", projectDocument.ProjectFile.FullName);
            
            Log.Verbose("===========================");
            foreach (PackageSource packageSource in projectDocument.ConfiguredPackageSources)
            {
                Log.Verbose(" - Project uses package source {PackageSourceName} ({PackageSourceUrl})",
                    packageSource.Name,
                    packageSource.Source
                );
            }

            Log.Verbose("===========================");
            if (projectDocument.HasMSBuildProject)
            {
                MSBuildObject[] msbuildObjects = projectDocument.MSBuildObjects.ToArray();
                Log.Verbose("MSBuild project loaded ({MSBuildObjectCount} MSBuild objects).", msbuildObjects.Length);

                foreach (MSBuildObject msbuildObject in msbuildObjects)
                {
                    Log.Verbose("{Type:l}: {Kind} {Name} spanning {XmlRange}",
                        msbuildObject.GetType().Name,
                        msbuildObject.Kind,
                        msbuildObject.Name,
                        msbuildObject.XmlRange
                    );
                }
            }
            else
                Log.Verbose("MSBuild project not loaded.");
        }

        /// <summary>
        ///     Called when a text document is opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override async Task OnDidChangeTextDocument(DidChangeTextDocumentParams parameters)
        {
            TextDocumentContentChangeEvent mostRecentChange = parameters.ContentChanges.LastOrDefault();
            if (mostRecentChange == null)
                return;

            string updatedDocumentText = mostRecentChange.Text;
            ProjectDocument projectDocument = await TryUpdateProjectDocument(parameters.TextDocument.Uri, updatedDocumentText);
            PublishDiagnostics(projectDocument);

            if (projectDocument.HasMSBuildProject)
            {
                MSBuildObject[] msbuildObjects = projectDocument.MSBuildObjects.ToArray();
                Log.Verbose("MSBuild project reloaded ({MSBuildObjectCount} MSBuild objects).", msbuildObjects.Length);

                foreach (MSBuildObject msbuildObject in msbuildObjects)
                {
                    Log.Verbose("{Type:l}: {Kind} {Name} spanning {XmlRange}:\n{@Object}",
                        msbuildObject.GetType().Name,
                        msbuildObject.Kind,
                        msbuildObject.Name,
                        msbuildObject.XmlRange,
                        msbuildObject
                    );
                }
            }
            else
                Log.Verbose("MSBuild project not loaded.");
        }

        /// <summary>
        ///     Called when a text document is saved.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override async Task OnDidSaveTextDocument(DidSaveTextDocumentParams parameters)
        {
            Log.Information("Reloading project...");
            ProjectDocument projectDocument = await GetProjectDocument(parameters.TextDocument.Uri, reload: true);
            PublishDiagnostics(projectDocument);

            if (!projectDocument.HasXml)
            {
                Log.Warning("Failed to reload project file {ProjectFilePath} (XML is invalid).", projectDocument.ProjectFile.FullName);

                return;
            }

            if (!projectDocument.HasMSBuildProject)
            {
                Log.Warning("Reloaded project file {ProjectFilePath} (XML is valid, but MSBuild project is not).", projectDocument.ProjectFile.FullName);

                return;
            }

            Log.Information("Successfully reloaded project {ProjectFilePath}.", projectDocument.ProjectFile.FullName);
        }

        /// <summary>
        ///     Called when a text document is opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override async Task OnDidCloseTextDocument(DidCloseTextDocumentParams parameters)
        {
            if (_projectDocuments.TryRemove(parameters.TextDocument.Uri, out ProjectDocument projectDocument))
            {
                using (await projectDocument.Lock.WriterLockAsync())
                {
                    ClearDiagnostics(projectDocument);
                    projectDocument.Unload();
                }
            }
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
            ProjectDocument projectDocument = await GetProjectDocument(parameters.TextDocument.Uri);
            
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

                // Match up the MSBuild item / property with its corresponding XML element / attribute.
                MSBuildObject msbuildObject = projectDocument.GetMSBuildObjectAtPosition(position);

                SyntaxNode elementOrAttribute = xmlNode.GetContainingElementOrAttribute();
                if (elementOrAttribute == null)
                    return null;

                Range range = elementOrAttribute.Span.ToNative(projectDocument.XmlPositions);
                Hover result = new Hover
                {
                    Range = range.ToLsp()
                };

                if (elementOrAttribute is IXmlElementSyntax element)
                {
                    if (msbuildObject is MSBuildProperty propertyFromElement)
                        result.Contents = GetHoverContent(propertyFromElement);
                    else if (msbuildObject is MSBuildUndefinedProperty undefinedPropertyFromElement)
                        result.Contents = GetHoverContent(undefinedPropertyFromElement, projectDocument);
                    else if (msbuildObject is MSBuildItemGroup itemGroupFromElement)
                        result.Contents = GetHoverContent(itemGroupFromElement);
                    else if (msbuildObject is MSBuildTarget targetFromElement)
                        result.Contents = GetHoverContent(targetFromElement);
                    else if (msbuildObject is MSBuildImport importFromElement)
                        result.Contents = GetHoverContent(importFromElement);
                    else
                        return null;
                }
                else if (elementOrAttribute is XmlAttributeSyntax attribute)
                {
                    if (msbuildObject is MSBuildItemGroup itemGroupFromAttribute)
                        result.Contents = GetHoverContent(itemGroupFromAttribute, attribute);
                    else if (msbuildObject is MSBuildSdkImport sdkImportFromAttribute)
                        result.Contents = GetHoverContent(sdkImportFromAttribute);
                    else if (msbuildObject is MSBuildImport importFromAttribute)
                        result.Contents = GetHoverContent(importFromAttribute);
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
            ProjectDocument projectDocument = await GetProjectDocument(parameters.TextDocument.Uri);

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
                Range attributeValueRange = attribute.ValueNode.Span
                    .ToNative(projectDocument.XmlPositions)
                    .Transform( // Trim off leading and trailing quotes.
                        moveStartColumns: 1,
                        moveEndColumns: -1
                    );
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
            ProjectDocument projectDocument = await GetProjectDocument(parameters.TextDocument.Uri);

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
            ProjectDocument projectDocument = await GetProjectDocument(parameters.TextDocument.Uri);

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
                                    Uri = UriHelper.CreateDocumentUri(importedProjectRoot.Location.File)
                                }
                            )
                            .ToArray();

                        return new LocationOrLocations(locations);
                    }
                    else if (msbuildObjectAtPosition is MSBuildImport importAtPosition)
                    {
                        // TODO: Parse imported project and determine location of root element (use that range instead).
                        return new LocationOrLocations(new Location
                        {
                            Range = Range.Empty.ToLsp(),
                            Uri = UriHelper.CreateDocumentUri(importAtPosition.ImportedProjectRoot.Location.File)
                        });
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Try to retrieve the current state for the specified project document.
        /// </summary>
        /// <param name="documentUri">
        ///     The project document URI.
        /// </param>
        /// <param name="reload">
        ///     Reload the project if it is already loaded?
        /// </param>
        /// <returns>
        ///     The project document.
        /// </returns>
        async Task<ProjectDocument> GetProjectDocument(Uri documentUri, bool reload = false)
        {
            string projectFilePath = documentUri.GetFileSystemPath();

            bool isNewProject = false;
            ProjectDocument projectDocument = _projectDocuments.GetOrAdd(documentUri, _ =>
            {
                isNewProject = true;

                return new ProjectDocument(documentUri, Log);
            });

            try
            {
                if (isNewProject || reload)
                {
                    using (await projectDocument.Lock.WriterLockAsync())
                    {
                        await projectDocument.Load();
                    }
                }
            }
            catch (XmlException invalidXml)
            {
                Log.Error("Error parsing project file {ProjectFilePath}: {ErrorMessage:l}",
                    projectFilePath,
                    invalidXml.Message
                );
            }
            catch (Exception loadError)
            {
                Log.Error(loadError, "Unexpected error loading file {ProjectFilePath}.", projectFilePath);
            }

            return projectDocument;
        }

        /// <summary>
        ///     Try to retrieve the current state for the specified project document.
        /// </summary>
        /// <param name="documentUri">
        ///     The project document URI.
        /// </param>
        /// <param name="reload">
        ///     Reload the project if it is already loaded?
        /// </param>
        /// <returns>
        ///     The project document.
        /// </returns>
        async Task<ProjectDocument> TryUpdateProjectDocument(Uri documentUri, string documentText)
        {
            ProjectDocument projectDocument;
            if (!_projectDocuments.TryGetValue(documentUri, out projectDocument))
            {
                Log.Error("Tried to update non-existent project with document URI {DocumentUri}.", documentUri);

                throw new InvalidOperationException($"Project with document URI '{documentUri}' is not loaded.");
            }

            try
            {
                using (await projectDocument.Lock.WriterLockAsync())
                {
                    projectDocument.Update(documentText);
                }
            }
            catch (Exception updateError)
            {
                Log.Error(updateError, "Failed to update project {ProjectFile}.", projectDocument.ProjectFile.FullName);
            }

            return projectDocument;
        }

        /// <summary>
        ///     Publish current diagnostics (if any) for the specified project document.
        /// </summary>
        /// <param name="projectDocument">
        ///     The project document.
        /// </param>
        void PublishDiagnostics(ProjectDocument projectDocument)
        {
            if (projectDocument == null)
                throw new ArgumentNullException(nameof(projectDocument));

            Server.PublishDiagnostics(new PublishDiagnosticsParams
            {
                Uri = projectDocument.DocumentUri,
                Diagnostics = projectDocument.Diagnostics.ToArray()
            });   
        }

        /// <summary>
        ///     Clear current diagnostics (if any) for the specified project document.
        /// </summary>
        /// <param name="projectDocument">
        ///     The project document.
        /// </param>
        void ClearDiagnostics(ProjectDocument projectDocument)
        {
            if (projectDocument == null)
                throw new ArgumentNullException(nameof(projectDocument));

            if (!projectDocument.HasDiagnostics)
                return;

            Server.PublishDiagnostics(new PublishDiagnosticsParams
            {
                Uri = projectDocument.DocumentUri,
                Diagnostics = new Lsp.Models.Diagnostic[0] // Overwrites existing diagnostics for this document with an empty list
            });   
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildProperty"/>.
        /// </summary>
        /// <param name="property">
        ///     The <see cref="MSBuildProperty"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        MarkedStringContainer GetHoverContent(MSBuildProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            
            return $"**Property**: {property.Name} = '{property.Value}'";
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildUndefinedProperty"/>.
        /// </summary>
        /// <param name="undefinedProperty">
        ///     The <see cref="MSBuildUndefinedProperty"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        MarkedStringContainer GetHoverContent(MSBuildUndefinedProperty undefinedProperty, ProjectDocument projectDocument)
        {
            if (undefinedProperty == null)
                throw new ArgumentNullException(nameof(undefinedProperty));
            
            string condition = undefinedProperty.PropertyElement.Condition;
            string expandedCondition = projectDocument.MSBuildProject.ExpandString(condition);

            return new MarkedStringContainer(
                $"**Property**: {undefinedProperty.Name} != '{undefinedProperty.Value}' (condition evaluates to false)",
                $"Condition:\n* Raw =`{condition}`\n* Evaluated = `{expandedCondition}`"
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildItemGroup"/>.
        /// </summary>
        /// <param name="itemGroup">
        ///     The <see cref="MSBuildItemGroup"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        MarkedStringContainer GetHoverContent(MSBuildItemGroup itemGroup)
        {
            if (itemGroup == null)
                throw new ArgumentNullException(nameof(itemGroup));
            
            if (itemGroup.Name == "PackageReference")
            {
                string packageVersion = itemGroup.GetFirstMetadataValue("Version");
                
                return $"**NuGet Package**: {itemGroup.FirstInclude} v{packageVersion}";
            }

            if (itemGroup.HasSingleItem)
                return $"**Item**: {itemGroup.OriginatingElement.ItemType}({itemGroup.FirstInclude})";

            string[] includes = itemGroup.Includes.ToArray();
            StringBuilder itemIncludeContent = new StringBuilder();
            itemIncludeContent.AppendLine(
                $"Include = `{itemGroup.OriginatingElement.Include}`  "
            );
            itemIncludeContent.AppendLine(
                $"Evaluates to {itemGroup.Items.Count} items:"
            );
            foreach (string include in includes.Take(5))
            {
                itemIncludeContent.AppendLine(
                    $"* {include}"
                );
            }
            if (includes.Length > 5)
                itemIncludeContent.AppendLine("* ...");

            return new MarkedStringContainer(
                $"**Items**: {itemGroup.OriginatingElement.ItemType}",
                itemIncludeContent.ToString()
            );  
        }

        /// <summary>
        ///     Get hover content for an attribute of an <see cref="MSBuildItemGroup"/>.
        /// </summary>
        /// <param name="itemGroup">
        ///     The <see cref="MSBuildItemGroup"/>.
        /// </param>
        /// <param name="attribute">
        ///     The attribute.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        MarkedStringContainer GetHoverContent(MSBuildItemGroup itemGroup, XmlAttributeSyntax attribute)
        {
            if (itemGroup == null)
                throw new ArgumentNullException(nameof(itemGroup));

            // TODO: Handle the "Condition" attribute.

            string metadataName = attribute.Name;
            if (String.Equals(metadataName, "Include"))
                metadataName = "Identity";

            if (itemGroup.Items.Count == 1)
            {
                string metadataValue = itemGroup.GetFirstMetadataValue(metadataName);
                
                return $"**Metadata**: {itemGroup.Name}({itemGroup.FirstInclude}).{metadataName} = '{metadataValue}'";
            }

            StringBuilder metadataValues = new StringBuilder();
            metadataValues.AppendLine("Values:");

            foreach (string metadataValue in itemGroup.GetMetadataValues(metadataName).Distinct())
            {
                metadataValues.AppendLine(
                    $"* {metadataValue}"
                );
            }

            return new MarkedStringContainer(
                $"**Metadata**: {itemGroup.Name}({itemGroup.FirstRawInclude}).{metadataName}",
                metadataValues.ToString()
            );
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildTarget"/>.
        /// </summary>
        /// <param name="target">
        ///     The <see cref="MSBuildTarget"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        MarkedStringContainer GetHoverContent(MSBuildTarget target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            
            return $"**Target**: {target.Name}";
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildImport"/>.
        /// </summary>
        /// <param name="import">
        ///     The <see cref="MSBuildImport"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        MarkedStringContainer GetHoverContent(MSBuildImport import)
        {
            if (import == null)
                throw new ArgumentNullException(nameof(import));
            
            string importedProject = import.ProjectImportElement.Project;
            Uri projectFileUri = UriHelper.CreateDocumentUri(import.ImportedProjectRoot.Location.File);

            return $"**Import**: [{importedProject}]({projectFileUri})";
        }

        /// <summary>
        ///     Get hover content for an <see cref="MSBuildSdkImport"/>.
        /// </summary>
        /// <param name="sdkImport">
        ///     The <see cref="MSBuildSdkImport"/>.
        /// </param>
        /// <returns>
        ///     The content.
        /// </returns>
        MarkedStringContainer GetHoverContent(MSBuildSdkImport sdkImport)
        {
            if (sdkImport == null)
                throw new ArgumentNullException(nameof(sdkImport));
            
            StringBuilder importedProjectFiles = new StringBuilder();
            foreach (string projectFile in sdkImport.ImportedProjectFiles)
                importedProjectFiles.AppendLine($"* Imports [{Path.GetFileName(projectFile)}]({UriHelper.CreateDocumentUri(projectFile)})");

            return new MarkedStringContainer(
                $"**SDK Import**: {sdkImport.Name}",
                importedProjectFiles.ToString()
            );
        }
    }
}
