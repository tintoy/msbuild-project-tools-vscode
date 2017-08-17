using Microsoft.Language.Xml;
using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Versioning;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MSBuild = Microsoft.Build.Evaluation;
    
namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using Documents;
    using Utilities;
    using XmlParser;

    // Note - you can get the workspace root path from Server.Client.RootPath

    /// <summary>
    ///     Handler for project file document events.
    /// </summary>
    public class ProjectDocumentHandler
        : TextDocumentHandler
    {
        /// <summary>
        ///     Documents for loaded project files.
        /// </summary>
        readonly ConcurrentDictionary<string, ProjectDocument> _projectDocuments = new ConcurrentDictionary<string, ProjectDocument>();

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
            string projectFilePath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (projectFilePath == null)
                return;

            ProjectDocument projectDocument = await TryLoadProjectDocument(projectFilePath);
            if (projectDocument == null)
            {
                Log.Warning("Failed to load project file {ProjectFilePath}.", projectFilePath);

                return;
            }

            Log.Information("Successfully loaded project {ProjectFilePath}.", projectFilePath);
            foreach (PackageSource packageSource in projectDocument.ConfiguredPackageSources)
            {
                Log.Information(" - Project uses package source {PackageSourceName} ({PackageSourceUrl})",
                    packageSource.Name,
                    packageSource.Source
                );
            }
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

            string projectFilePath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (projectFilePath == null)
                return;

            string updatedDocumentText = mostRecentChange.Text;
            await TryUpdateProjectDocument(projectFilePath, updatedDocumentText);
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
            string projectFilePath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (projectFilePath == null)
                return;

            Log.Information("Reloading project {ProjectFilePath}...", projectFilePath);
            ProjectDocument projectDocument = await TryLoadProjectDocument(projectFilePath, reload: true);
            if (projectDocument == null)
            {
                Log.Warning("Failed to reload project file {ProjectFilePath}.", projectFilePath);

                return;
            }

            Log.Information("Successfully reloaded project {ProjectFilePath}.", projectFilePath);
            foreach (PackageSource packageSource in projectDocument.ConfiguredPackageSources)
            {
                Log.Information(" - Project uses package source {PackageSourceName} ({PackageSourceUrl})",
                    packageSource.Name,
                    packageSource.Source
                );
            }
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
            string projectFilePath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (projectFilePath == null)
                return;

            if (_projectDocuments.TryRemove(projectFilePath, out ProjectDocument projectDocument))
            {
                using (await projectDocument.Lock.WriterLockAsync())
                {
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
            string projectFilePath = parameters.TextDocument.Uri.GetFileSystemPath();
            ProjectDocument projectDocument = await TryLoadProjectDocument(projectFilePath);
            if (projectDocument == null || !projectDocument.HasMSBuildProject)
                return null;

            Position position = parameters.Position.ToNative();

            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                // First, match up the MSBuild item / property with its corresponding XML element / attribute.

                // object msbuildObjectAtPosition = projectDocument.GetMSBuildObjectAtPosition(position);
                // if (msbuildObjectAtPosition == null)
                //     return null;

                SyntaxNode xmlAtPosition = projectDocument.GetXmlAtPosition(position);
                if (xmlAtPosition == null)
                    return null;

                SyntaxNode elementOrAttribute = xmlAtPosition.GetContainingAttributeOrElement();
                if (elementOrAttribute is IXmlElementSyntax element)
                {
                    Position startPosition = projectDocument.XmlPositions.GetPosition(elementOrAttribute.Span.Start);
                    Position endPosition = projectDocument.XmlPositions.GetPosition(elementOrAttribute.Span.End);

                    return new Hover
                    {
                        Contents = $"Element '{element.Name.Name}'",
                        Range = new Lsp.Models.Range
                        {                            
                            Start = startPosition.ToLsp(),
                            End = endPosition.ToLsp()
                        }
                    };
                }

                if (elementOrAttribute is XmlAttributeSyntax attribute)
                {
                    Position startPosition = projectDocument.XmlPositions.GetPosition(elementOrAttribute.Span.Start);
                    Position endPosition = projectDocument.XmlPositions.GetPosition(elementOrAttribute.Span.End);

                    return new Hover
                    {
                        Contents = $"Attribute '{attribute.Name}' (='{attribute.Value}')",
                        Range = new Lsp.Models.Range
                        {                            
                            Start = startPosition.ToLsp(),
                            End = endPosition.ToLsp()
                        }
                    };
                }

                // Range xmlRange = xmlAtPosition.Annotation<NodeLocation>().Range;

                // MSBuild.ProjectProperty propertyAtPosition = msbuildObjectAtPosition as MSBuild.ProjectProperty;
                // if (propertyAtPosition != null)
                // {
                //     return new Hover
                //     {
                //         Range = xmlRange.ToLsp(),
                //         Contents = $"Property '{propertyAtPosition.Name}' (='{propertyAtPosition.EvaluatedValue}')"
                //     };
                // }

                // MSBuild.ProjectItem itemAtPosition = msbuildObjectAtPosition as MSBuild.ProjectItem;
                // if (itemAtPosition != null)
                // {
                //     // Are we on a metadata attribute?
                //     if (xmlAtPosition is XAttribute attribute)
                //     {
                //         string metadataName = attribute.Name.LocalName;
                //         if (String.Equals(metadataName, "Include"))
                //             metadataName = "Identity";

                //         string metadataValue = itemAtPosition.GetMetadataValue(metadataName);
                        
                //         return new Hover
                //         {
                //             Range = xmlRange.ToLsp(),
                //             Contents = $"Metadata '{metadataName}' of {attribute.Parent.Name} item '{itemAtPosition.EvaluatedInclude}' (='{metadataValue}')"
                //         };
                //     }
                //     else if (xmlAtPosition is XElement element)
                //     {
                //         return new Hover
                //         {
                //             Range = xmlRange.ToLsp(),
                //             Contents = $"{element.Name} item '{itemAtPosition.EvaluatedInclude}'"
                //         };
                //     }

                    return null;
                }

                // No idea what (if any) part of the project this is, so just display a not-so-informative tooltip.
                // XmlNodeType objectType = xmlAtPosition.NodeType;
                // string objectNameLabel = "Unknown";
                // string objectValueLabel = "";
                // objectType = xmlAtPosition.NodeType;
                // switch (xmlAtPosition)
                // {
                //     case XElement element:
                //     {
                //         objectNameLabel = element.Name.LocalName;

                //         break;
                //     }
                //     case XAttribute attribute:
                //     {
                //         objectNameLabel = attribute.Name.LocalName;
                //         objectValueLabel = $" (='{attribute.Value}')";

                //         break;
                //     }
                // }

                // return new Hover
                // {
                //     Range = xmlRange.ToLsp(),
                //     Contents = $"{objectType} '{objectNameLabel}'{objectValueLabel}"
                // };
            // }
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
            string projectFilePath = parameters.TextDocument.Uri.GetFileSystemPath();
            ProjectDocument projectDocument = await TryLoadProjectDocument(projectFilePath);
            if (projectDocument == null || !projectDocument.HasXml)
                return null;

            Position position = parameters.Position.ToNative();

            return null;

            // List<CompletionItem> completionItems = new List<CompletionItem>();
            // using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            // {
            //     // Are we on an attribute?
            //     XAttribute attributeAtPosition = projectDocument.GetXmlAtPosition<XAttribute>(position);
            //     if (attributeAtPosition == null)
            //         return null;

            //     // Must be a PackageReference element.
            //     if (!String.Equals(attributeAtPosition.Parent.Name.LocalName, "PackageReference", StringComparison.OrdinalIgnoreCase))
            //         return null;

            //     // Are we on the attribute's value?
            //     AttributeLocation attributeLocation = attributeAtPosition.Annotation<AttributeLocation>();
            //     if (!attributeLocation.ValueRange.Contains(position))
            //         return null;

            //     try
            //     {
            //         if (attributeAtPosition.Name == "Include")
            //         {
            //             SortedSet<string> packageIds = await projectDocument.SuggestPackageIds(attributeAtPosition.Value, cancellationToken);
            //             completionItems.AddRange(
            //                 packageIds.Select(packageId => new CompletionItem
            //                 {
            //                     Label = packageId,
            //                     Kind = CompletionItemKind.Module,
            //                     TextEdit = new TextEdit
            //                     {
            //                         Range = attributeLocation.ValueRange.ToLsp(),
            //                         NewText = packageId
            //                     }
            //                 })
            //             );
            //         }
            //         else if (attributeAtPosition.Name == "Include")
            //         {
            //             SortedSet<NuGetVersion> packageIds = await projectDocument.SuggestPackageVersions(attributeAtPosition.Value, cancellationToken);
            //             completionItems.AddRange(
            //                 packageIds.Select(packageVersion => new CompletionItem
            //                 {
            //                     Label = packageVersion.ToNormalizedString(),
            //                     Kind = CompletionItemKind.Field,
            //                     TextEdit = new TextEdit
            //                     {
            //                         Range = attributeLocation.ValueRange.ToLsp(),
            //                         NewText = packageVersion.ToNormalizedString()
            //                     }
            //                 })
            //             );
            //         }
            //         else
            //             return null; // No completions.
            //     }
            //     catch (AggregateException aggregateSuggestionError)
            //     {
            //         foreach (Exception suggestionError in aggregateSuggestionError.Flatten().InnerExceptions)
            //         {
            //             Log.Error(suggestionError, "Failed to provide completions.");    
            //         }
            //     }
            //     catch (Exception suggestionError)
            //     {
            //         Log.Error(suggestionError, "Failed to provide completions.");
            //     }
            // }

            // return new CompletionList(completionItems,
            //     isIncomplete: completionItems.Count >= 20 // Default page size.
            // );
        }

        /// <summary>
        ///     Try to retrieve the current state for the specified project document.
        /// </summary>
        /// <param name="projectFilePath">
        ///     The full path to the project document.
        /// </param>
        /// <param name="reload">
        ///     Reload the project if it is already loaded?
        /// </param>
        /// <returns>
        ///     The project document, or <c>null</c> if the project is not already loaded.
        /// </returns>
        async Task<ProjectDocument> TryUpdateProjectDocument(string projectFilePath, string documentText)
        {
            ProjectDocument projectDocument;
            if (!_projectDocuments.TryGetValue(projectFilePath, out projectDocument))
                return null;

            try
            {
                using (await projectDocument.Lock.WriterLockAsync())
                {
                    projectDocument.Update(documentText);
                }
            }
            catch (Exception updateError)
            {
                Log.Error(updateError, "Failed to update project {ProjectFile}.", projectFilePath);
            }

            return projectDocument;
        }

        /// <summary>
        ///     Try to retrieve the current state for the specified project document.
        /// </summary>
        /// <param name="projectFilePath">
        ///     The full path to the project document.
        /// </param>
        /// <param name="reload">
        ///     Reload the project if it is already loaded?
        /// </param>
        /// <returns>
        ///     The project document, or <c>null</c> if the project could not be loaded.
        /// </returns>
        async Task<ProjectDocument> TryLoadProjectDocument(string projectFilePath, bool reload = false)
        {
            try
            {
                bool isNewProject = false;
                ProjectDocument projectDocument = _projectDocuments.GetOrAdd(projectFilePath, _ =>
                {
                    isNewProject = true;
                     
                    return new ProjectDocument(projectFilePath, Log);
                });

                if (isNewProject || reload)
                {
                    using (await projectDocument.Lock.WriterLockAsync())
                    {
                        await projectDocument.Load();
                    }
                }

                return projectDocument;
            }
            catch (XmlException invalidXml)
            {
                Log.Error("Error parsing project file {ProjectFilePath}: {ErrorMessage}",
                    projectFilePath,
                    invalidXml.Message
                );
            }
            catch (Exception loadError)
            {
                Log.Error(loadError, "Unexpected error loading file {ProjectFilePath}.", projectFilePath);
            }

            return null;
        }
    }
}
