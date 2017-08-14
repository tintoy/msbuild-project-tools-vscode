using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer
{
    /// <summary>
    ///     The base class for language server text-document event handlers.
    /// </summary>
    class TextDocumentHandler
        : ITextDocumentSyncHandler
    {
        /// <summary>
        ///     The document selector that describes documents targeted by the handler.
        /// </summary>
        readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter
            {
                Pattern = "**/*.csproj",
                Language = "xml"
            }
        );

        /// <summary>
        ///     The handler's logger.
        /// </summary>
        readonly ILogger _server;

        /// <summary>
        ///     Create a new <see cref="TextDocumentHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public TextDocumentHandler(ILanguageServer server, ILogger logger)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            
            Server = server;
            _server = logger.ForContext<TextDocumentHandler>();
        }

        /// <summary>
        ///     Options that control synchronisation.
        /// </summary>
        public TextDocumentSyncOptions Options { get; } = new TextDocumentSyncOptions
        {
            WillSaveWaitUntil = false,
            WillSave = true,
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions
            {
                IncludeText = true
            },
            OpenClose = true
        };

        /// <summary>
        ///     The language server.
        /// </summary>
        ILanguageServer Server { get; }

        /// <summary>
        ///     The server's synchronisation capabilities.
        /// </summary>
        SynchronizationCapability Capabilities { get; set; }

        /// <summary>
        ///     Handle a document being opened.
        /// </summary>
        /// <param name="notification">
        ///     The notification data.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidOpenTextDocumentParams notification)
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
        ///     Handle a document being closed.
        /// </summary>
        /// <param name="notification">
        ///     The notification data.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidCloseTextDocumentParams notification)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Handle a change in document text.
        /// </summary>
        /// <param name="notification">
        ///     The notification data.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidChangeTextDocumentParams notification)
        {
            string documentPath = notification.TextDocument.Uri.GetFileSystemPath();
            int changeCount = notification.ContentChanges.Count();
            Server.LogMessage(new LogMessageParams
            {
                Type = MessageType.Log,
                Message = $"Document '{documentPath}' changed."
            });
            foreach (TextDocumentContentChangeEvent change in notification.ContentChanges)
            {
                Server.LogMessage(new LogMessageParams
                {
                    Type = MessageType.Log,
                    Message = $"Changed {change.RangeLength} characters in '{documentPath}' between ({change.Range.Start.Line}, {change.Range.Start.Character}) to ({change.Range.End.Line}, {change.Range.End.Character})."
                });
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Handle a document being saved.
        /// </summary>
        /// <param name="notification">
        ///     The notification data.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidSaveTextDocumentParams notification)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Get global registration options for handling document-text events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector
            };
        }

        /// <summary>
        ///     Get registration options for handling document-text change events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                SyncKind = Options.Change
            };
        }

        /// <summary>
        ///     Get registration options for handling document-text save events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                IncludeText = Options.Save.IncludeText
            };
        }
        
        /// <summary>
        ///     Called to inform the handler of the language server's document-synchronisation capabilities.
        /// </summary>
        /// <param name="capabilities"></param>
        public void SetCapability(SynchronizationCapability capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            Capabilities = capabilities;
        }

        /// <summary>
        ///     Get attributes for the specified text document.
        /// </summary>
        /// <param name="documentUri">
        ///     The document URI.
        /// </param>
        /// <returns>
        ///     The document attributes.
        /// </returns>
        public TextDocumentAttributes GetTextDocumentAttributes(Uri documentUri)
        {
            if (documentUri == null)
                throw new ArgumentNullException(nameof(documentUri));

            return new TextDocumentAttributes(documentUri, "xml");
        }

        /// <summary>
        ///     Log an information message.
        /// </summary>
        /// <param name="message">
        ///     The message (Serilog-style format string).
        /// </param>
        /// <param name="propertyValues">
        ///     Values for message properties (if any).
        /// </param>
        void LogInformation(string message, params object[] propertyValues)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _server.Information(message, propertyValues);

            // TODO: _server.LogMessage
        }

        /// <summary>
        ///     Log a warning message.
        /// </summary>
        /// <param name="message">
        ///     The message (Serilog-style format string).
        /// </param>
        /// <param name="propertyValues">
        ///     Values for message properties (if any).
        /// </param>
        void LogWarning(string message, params object[] propertyValues)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _server.Warning(message, propertyValues);

            // TODO: _server.LogMessage
        }

        /// <summary>
        ///     Log an error message.
        /// </summary>
        /// <param name="message">
        ///     The message (Serilog-style format string).
        /// </param>
        /// <param name="propertyValues">
        ///     Values for message properties (if any).
        /// </param>
        void LogError(string message, params object[] propertyValues)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _server.Error(message, propertyValues);

            // TODO: _server.LogMessage
        }

        /// <summary>
        ///     Log an exception.
        /// </summary>
        /// <param name="exception">
        ///     The exception.
        /// </param>
        /// <param name="message">
        ///     An error message (Serilog-style format string) to go with the exception.
        /// </param>
        /// <param name="propertyValues">
        ///     Values for message properties (if any).
        /// </param>
        void LogException(Exception exception, string message, params object[] propertyValues)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            _server.Error(exception, message, propertyValues);

            // TODO: _server.LogMessage
        }
    }
}
