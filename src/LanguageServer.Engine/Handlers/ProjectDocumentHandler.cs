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

            ProjectDocument projectDocument = TryLoadProjectDocument(documentPath);
            if (projectDocument == null)
            {
                Log.Warning("Failed to load project file '{DocumentPath}'.", documentPath);

                return Task.CompletedTask;
            }

            Log.Information("Successfully loaded project '{DocumentPath}'.", documentPath);

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
        protected override Task<Hover> OnHover(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            ProjectDocument projectDocument;
            if (!_projectDocuments.TryGetValue(documentPath, out projectDocument) || !projectDocument.IsLoaded)
                return NoHover;

            Position position = Position.FromZeroBased(
                parameters.Position.Line,
                parameters.Position.Character
            ).ToOneBased();

            XObject objectAtPosition = projectDocument.GetXmlAtPosition(position);
            if (objectAtPosition == null)
                return NoHover;

            XmlNodeType objectType = objectAtPosition.NodeType;
            string objectName;
            string objectValue = "";            
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
                    objectValue = $" (='{attribute.Value}')";

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
                Contents = $"{objectType} '{objectName}'{objectValue}",
            });
        }

        /// <summary>
        ///     Try to load the latest state for the specified project document.
        /// </summary>
        /// <param name="documentPath">
        ///     The full path to the project document.
        /// </param>
        /// <returns>
        ///     The project document, or <c>null</c> if the project could not be loaded.
        /// </returns>
        ProjectDocument TryLoadProjectDocument(string documentPath)
        {
            try
            {
                ProjectDocument projectDocument;
                if (!_projectDocuments.TryGetValue(documentPath, out projectDocument))
                {
                    projectDocument = new ProjectDocument(documentPath);
                    _projectDocuments.Add(documentPath, projectDocument);
                }

                projectDocument.Load();

                return projectDocument;
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

            return null;
        }
    }
}
