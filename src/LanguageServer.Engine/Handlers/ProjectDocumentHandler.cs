using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
    
namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using System.Threading;
    using Documents;
    using Utilities;
    using XmlParser;

    /// <summary>
    ///     Handler for project file document events.
    /// </summary>
    public class ProjectDocumentHandler
        : TextDocumentHandler
    {
        /// <summary>
        ///     A pre-completed task representing a null hover result.
        /// </summary>
        static readonly Task<Hover> NoHover = Task.FromResult<Hover>(null);

        /// <summary>
        ///     Documents for loaded project files.
        /// </summary>
        readonly Dictionary<string, ProjectDocument> _projectDocuments = new Dictionary<string, ProjectDocument>();

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
            Options.Change = TextDocumentSyncKind.Incremental;
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
        protected override Task OnDidOpenTextDocument(DidOpenTextDocumentParams parameters)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return Task.CompletedTask;

            Log.Information("Opened project file '{DocumentPath}'.", documentPath);

            ProjectDocument projectDocument = new ProjectDocument(documentPath);
            _projectDocuments.Add(documentPath, projectDocument);

            if (!TryLoadProjectDocument(documentPath))
            {
                Log.Warning("Failed to parse project file '{DocumentPath}'.", documentPath);

                return Task.CompletedTask;
            }

            XElement[] packageReferenceElements = projectDocument.Xml.Descendants("PackageReference").ToArray();
            Log.Information("Successfully parsed XML for project '{DocumentPath}' ({PackageReferenceCount} package references detected).",
                documentPath,
                packageReferenceElements.Length
            );
            foreach (XElement packageReferenceElement in packageReferenceElements)
            {
                NodeLocation elementLocation = packageReferenceElement.Annotation<NodeLocation>();
                if (elementLocation == null)
                    continue;

                Log.Information("Found PackageReference element spanning ({StartLine},{StartColumn}) to ({EndLine},{EndColumn}).",
                    elementLocation.Start.LineNumber,
                    elementLocation.Start.ColumnNumber,
                    elementLocation.End.LineNumber,
                    elementLocation.End.ColumnNumber
                );
            }

            return Task.CompletedTask;
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
        protected override Task OnDidCloseTextDocument(DidCloseTextDocumentParams parameters)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return Task.CompletedTask;

            _projectDocuments.Remove(documentPath);

            return Task.CompletedTask;
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
        protected override Task OnDidChangeTextDocument(DidChangeTextDocumentParams parameters)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return Task.CompletedTask;

            TextDocumentContentChangeEvent[] contentChanges = parameters.ContentChanges.ToArray();
            Log.Information("Document '{DocumentPath}' changed ({ChangeCount} changes detected).",
                documentPath,
                contentChanges.Length
            );
            if (contentChanges.Length == 0)
                return Task.CompletedTask;

            foreach (TextDocumentContentChangeEvent change in contentChanges)
            {
                Log.Verbose("Changed {RangeLength} characters in '{DocumentPath}' between ({StartRangeLine}, {StartRangeColumn}) to ({EndRangeLine}, {EndRangeColumn}).",
                    change.RangeLength,
                    documentPath,
                    change.Range?.Start?.Line ?? 0,
                    change.Range?.Start.Character ?? 0,
                    change.Range?.End?.Line ?? 0,
                    change.Range?.End?.Character ?? 0
                );
                Log.Verbose("ChangeEvent: {EventData}", JsonConvert.SerializeObject(change));
            }

            return Task.CompletedTask;
        }

        protected override Task<Hover> RequestHover(TextDocumentPositionParams parameters, CancellationToken token)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            ProjectDocument projectDocument;
            if (!_projectDocuments.TryGetValue(documentPath, out projectDocument) || !projectDocument.IsLoaded)
                return NoHover;

            var position = Position.FromZeroBased(
                (int)parameters.Position.Line,
                (int)parameters.Position.Character
            );

            position = position.ToOneBased();

            XObject objectAtPosition = projectDocument.GetXmlAtPosition(position);
            if (objectAtPosition == null)
                return NoHover;

            XmlNodeType objectType = objectAtPosition.NodeType;
            string objectName;
            switch (objectAtPosition)
            {
                case XElement element:
                {
                    objectName = element.Name.LocalName;

                    break;
                }
                case XAttribute attribute:
                {
                    objectName = attribute.Name.LocalName;

                    break;
                }
                default:
                {
                    objectName = "Unknown";

                    break;
                }
            }

            return Task.FromResult(new Hover
            {
                Range = objectAtPosition.Annotation<NodeLocation>().Range.ToLspModel(),
                Contents = $"{objectType} object '{objectName}'",
            });
        }

        /// <summary>
        ///     Try to load the latest state for the specified project document.
        /// </summary>
        /// <param name="documentPath">
        ///     The full path to the project document.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the project was loaded; otherwise, <c>false</c>.
        /// </returns>
        bool TryLoadProjectDocument(string documentPath)
        {
            try
            {
                ProjectDocument projectDocument;
                if (!_projectDocuments.TryGetValue(documentPath, out projectDocument))
                    return false;

                projectDocument.Load();

                return true;
            }
            catch (XmlException invalidXml)
            {
                Log.Error("Error parsing project file '{DocumentPath}': {ErrorMessage}",
                    documentPath,
                    invalidXml.Message
                );
            }
            catch (Exception loadError)
            {
                Log.Error(loadError, "Unexpected error loading file {DocumentPath}.", documentPath);
            }

            return false;
        }
    }
}
