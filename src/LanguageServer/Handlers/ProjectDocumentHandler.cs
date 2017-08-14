using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using Newtonsoft.Json;
    using Utilities;

    /// <summary>
    ///     Handler for project file document events.
    /// </summary>
    public class ProjectDocumentHandler
        : TextDocumentHandler
    {
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
        ///     Create a new <see cref="ProjectDocumentHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public ProjectDocumentHandler(ILanguageServer server)
            : base(server)
        {
            Options.Change = TextDocumentSyncKind.Incremental;
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
        protected override Task OnDidOpenTextDocument(DidOpenTextDocumentParams notification)
        {
            string documentPath = notification.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return Task.CompletedTask;

            LogInformation("Opened document '{0}'.", documentPath);

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

            LogInformation("Document '{0}' changed ({1} changes).",
                documentPath,
                parameters.ContentChanges.Count()
            );
            foreach (TextDocumentContentChangeEvent change in parameters.ContentChanges)
            {
                LogVerbose("Changed {0} characters in '{1}' between ({2}, {3}) to ({4}, {5}).",
                    change.RangeLength,
                    documentPath,
                    change.Range?.Start?.Line ?? 0,
                    change.Range?.Start.Character ?? 0,
                    change.Range?.End?.Line ?? 0,
                    change.Range?.End?.Character ?? 0
                );
                LogVerbose("ChangeEvent: {0}", JsonConvert.SerializeObject(change));
            }

            return Task.CompletedTask;
        }
    }
}
