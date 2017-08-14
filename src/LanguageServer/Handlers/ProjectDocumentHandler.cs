using Lsp;
using Lsp.Models;
using Lsp.Protocol;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
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
            Server.LogMessage(new LogMessageParams
            {
                Type = MessageType.Log,
                Message = $"Opened document '{documentPath}'."
            });

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
            int changeCount = parameters.ContentChanges.Count();
            Server.LogMessage(new LogMessageParams
            {
                Type = MessageType.Log,
                Message = $"Document '{documentPath}' changed."
            });
            foreach (TextDocumentContentChangeEvent change in parameters.ContentChanges)
            {
                Server.LogMessage(new LogMessageParams
                {
                    Type = MessageType.Log,
                    Message = $"Changed {change.RangeLength} characters in '{documentPath}' between ({change.Range.Start.Line}, {change.Range.Start.Character}) to ({change.Range.End.Line}, {change.Range.End.Character})."
                });
            }

            return Task.CompletedTask;
        }
    }
}
