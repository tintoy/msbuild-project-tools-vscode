using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
    
namespace MSBuildProjectTools.LanguageServer.Handlers
{
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
            ProjectDocument projectDocument = TryLoadProjectDocument(documentPath);
            if (projectDocument == null)
                return NoHover;

            Position position = Position.FromZeroBased(
                parameters.Position.Line,
                parameters.Position.Character
            ).ToOneBased();

            XObject objectAtPosition = projectDocument.GetXmlAtPosition(position);
            if (objectAtPosition == null)
                return NoHover;

            // Display a not-so-informative tooltip (this is just to prove that our language server is correctly servicing hover requests).

            XmlNodeType objectType = objectAtPosition.NodeType;
            string objectNameLabel;
            string objectValueLabel = "";            
            switch (objectAtPosition)
            {
                case XElement element:
                {
                    objectNameLabel = element.Name.LocalName;

                    break;
                }
                case XAttribute attribute:
                {
                    objectNameLabel = attribute.Name.LocalName;
                    objectValueLabel = $" (='{attribute.Value}')";

                    break;
                }
                default:
                {
                    objectNameLabel = "Unknown";

                    break;
                }
            }

            return Task.FromResult(new Hover
            {
                Range = objectAtPosition.Annotation<NodeLocation>().Range.ToLspModel(),
                Contents = $"{objectType} '{objectNameLabel}'{objectValueLabel}",
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
