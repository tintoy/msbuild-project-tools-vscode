using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
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
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return;

            ProjectDocument projectDocument = await TryLoadProjectDocument(documentPath);
            if (projectDocument == null)
            {
                Log.Warning("Failed to load project file '{DocumentPath}'.", documentPath);

                return;
            }

            Log.Information("Successfully loaded project '{DocumentPath}'.", documentPath);
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

            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return;

            string updatedDocumentText = mostRecentChange.Text;
            ProjectDocument reloadedProjectDocument = await TryUpdateProjectDocument(documentPath, updatedDocumentText);
            if (reloadedProjectDocument == null)
                Log.Warning("Failed to update project '{DocumentPath}'.", documentPath);
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
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return;

            if (_projectDocuments.TryRemove(documentPath, out ProjectDocument projectDocument))
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
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            ProjectDocument projectDocument = await TryLoadProjectDocument(documentPath);
            if (projectDocument == null)
                return null;

            Position position = Position.FromZeroBased(
                parameters.Position.Line,
                parameters.Position.Character
            ).ToOneBased();

            Range nodeRange;
            XmlNodeType objectType;
            string objectNameLabel;
            string objectValueLabel = "";

            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                XObject objectAtPosition = projectDocument.GetXmlAtPosition(position);
                if (objectAtPosition == null)
                    return null;

                nodeRange = objectAtPosition.Annotation<NodeLocation>().Range;

                // Display a not-so-informative tooltip (this is just to prove that our language server is correctly servicing hover requests).

                objectType = objectAtPosition.NodeType;
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
            }

            return new Hover
            {
                Range = nodeRange.ToLspModel(),
                Contents = $"{objectType} '{objectNameLabel}'{objectValueLabel}",
            };
        }

        /// <summary>
        ///     Try to retrieve the current state for the specified project document.
        /// </summary>
        /// <param name="documentPath">
        ///     The full path to the project document.
        /// </param>
        /// <param name="reload">
        ///     Reload the project if it is already loaded?
        /// </param>
        /// <returns>
        ///     The project document, or <c>null</c> if the project could not be loaded.
        /// </returns>
        async Task<ProjectDocument> TryUpdateProjectDocument(string documentPath, string documentText)
        {
            ProjectDocument projectDocument;
            if (!_projectDocuments.TryGetValue(documentPath, out projectDocument))
                return null;

            using (await projectDocument.Lock.WriterLockAsync())
            {
                projectDocument.Update(documentText);
            }

            return projectDocument;
        }

        /// <summary>
        ///     Try to retrieve the current state for the specified project document.
        /// </summary>
        /// <param name="documentPath">
        ///     The full path to the project document.
        /// </param>
        /// <param name="reload">
        ///     Reload the project if it is already loaded?
        /// </param>
        /// <returns>
        ///     The project document, or <c>null</c> if the project could not be loaded.
        /// </returns>
        async Task<ProjectDocument> TryLoadProjectDocument(string documentPath, bool reload = false)
        {
            try
            {
                bool isNewProject = false;
                ProjectDocument projectDocument = _projectDocuments.GetOrAdd(documentPath, _ =>
                {
                    isNewProject = true;
                     
                    return new ProjectDocument(documentPath);
                });

                if (isNewProject || reload)
                {
                    using (await projectDocument.Lock.WriterLockAsync())
                    {
                        projectDocument.Load();
                    }
                }

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
