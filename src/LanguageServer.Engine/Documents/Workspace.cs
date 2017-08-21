using Lsp.Models;
using Lsp.Protocol;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace MSBuildProjectTools.LanguageServer.Documents
{
    using Utilities;

    /// <summary>
    ///     The workspace that holds project documents.
    /// </summary>
    public class Workspace
    {
        /// <summary>
        ///     Documents for loaded project, keyed by document URI.
        /// </summary>
        readonly ConcurrentDictionary<Uri, ProjectDocument> _projectDocuments = new ConcurrentDictionary<Uri, ProjectDocument>();
        
        /// <summary>
        ///     Create a new <see cref="Workspace"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="configuration">
        ///     The language server configuration.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public Workspace(Lsp.ILanguageServer server, Configuration configuration, ILogger logger)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            
            Server = server;
            Log = logger.ForContext<Workspace>();
        }

        /// <summary>
        ///     The root directory for the workspace.
        /// </summary>
        public string RootDirectory => Server.Client.RootPath;

        /// <summary>
        ///     The master project (if any).
        /// </summary>
        /// <remarks>
        ///     TODO: Make this selectable from the editor (get the extension to show a pick-list of open projects).
        /// </remarks>
        MasterProjectDocument MasterProject { get; set; }

        /// <summary>
        ///     The language server.
        /// </summary>
        Lsp.ILanguageServer Server { get; }

        /// <summary>
        ///     The language server configuration.
        /// </summary>
        Configuration Configuration { get; }

        /// <summary>
        ///     The workspace logger.
        /// </summary>
        ILogger Log { get; }

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
        public async Task<ProjectDocument> GetProjectDocument(Uri documentUri, bool reload = false)
        {
            string projectFilePath = VSCodeDocumentUri.GetFileSystemPath(documentUri);

            bool isNewProject = false;
            ProjectDocument projectDocument = _projectDocuments.GetOrAdd(documentUri, _ =>
            {
                isNewProject = true;

                if (MasterProject == null)
                    return MasterProject = new MasterProjectDocument(documentUri, Log);

                SubProjectDocument subProject = new SubProjectDocument(documentUri, Log, MasterProject);
                MasterProject.AddSubProject(subProject);

                return subProject;
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
        /// <param name="documentText">
        ///     The new document text.
        /// </param>
        /// <returns>
        ///     The project document.
        /// </returns>
        public async Task<ProjectDocument> TryUpdateProjectDocument(Uri documentUri, string documentText)
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
        public void PublishDiagnostics(ProjectDocument projectDocument)
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
        public void ClearDiagnostics(ProjectDocument projectDocument)
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
        ///     Remove a project document from the workspace.
        /// </summary>
        /// <param name="documentUri">
        ///     The document URI.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> that resolves to <c>true</c> if the document was removed to the workspace; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> RemoveProjectDocument(Uri documentUri)
        {
            if (documentUri == null)
                throw new ArgumentNullException(nameof(documentUri));
            
            ProjectDocument projectDocument;
            if (!_projectDocuments.TryRemove(documentUri, out projectDocument))
                return false;
            
            if (MasterProject == projectDocument)
                MasterProject = null;                

            using (await projectDocument.Lock.WriterLockAsync())
            {
                ClearDiagnostics(projectDocument);

                projectDocument.Unload();
            }

            return true;
        }
    }
}
