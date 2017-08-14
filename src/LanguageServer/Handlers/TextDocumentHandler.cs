using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Linq;
using JsonRpc;
using System.Threading;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    /// <summary>
    ///     The base class for language server text-document event handlers.
    /// </summary>
    public abstract class TextDocumentHandler
        : ITextDocumentSyncHandler
    {
        /// <summary>
        ///     The handler's logger.
        /// </summary>
        readonly ILogger _logger;

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
            _logger = logger.ForContext(GetType());
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
        ///     The document selector that describes documents targeted by the handler.
        /// </summary>
        protected abstract DocumentSelector DocumentSelector { get; }

        /// <summary>
        ///     The language server.
        /// </summary>
        protected ILanguageServer Server { get; }

        /// <summary>
        ///     The server's synchronisation capabilities.
        /// </summary>
        protected SynchronizationCapability Capabilities { get; private set; }

        /// <summary>
        ///     Called when a text document is opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected virtual Task OnDidOpenTextDocument(DidOpenTextDocumentParams parameters) => Task.CompletedTask;

        /// <summary>
        ///     Called when a text document is closed.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected virtual Task OnDidCloseTextDocument(DidCloseTextDocumentParams parameters) => Task.CompletedTask;
        
        /// <summary>
        ///     Called when a text document is saved.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected virtual Task OnDidSaveTextDocument(DidSaveTextDocumentParams parameters) => Task.CompletedTask;

        /// <summary>
        ///     Called when a text document is changed.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected virtual Task OnDidChangeTextDocument(DidChangeTextDocumentParams parameters) => Task.CompletedTask;

        /// <summary>
        ///     Get global registration options for handling document events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        protected virtual TextDocumentRegistrationOptions RegistrationOptions
        {
            get => new TextDocumentRegistrationOptions
            {
                DocumentSelector = DocumentSelector
            };
        }

        /// <summary>
        ///     Get registration options for handling document-change events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        protected virtual TextDocumentChangeRegistrationOptions DocumentChangeRegistrationOptions
        {
            get => new TextDocumentChangeRegistrationOptions
            {
                DocumentSelector = DocumentSelector,
                SyncKind = Options.Change
            };
        }

        /// <summary>
        ///     Get registration options for handling document save events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        protected virtual TextDocumentSaveRegistrationOptions DocumentSaveRegistrationOptions
        {
            get => new TextDocumentSaveRegistrationOptions
            {
                DocumentSelector = DocumentSelector,
                IncludeText = Options.Save.IncludeText
            };
        }

        /// <summary>
        ///     Handle a document being opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidOpenTextDocumentParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            return OnDidOpenTextDocument(parameters);
        }

        /// <summary>
        ///     Handle a document being closed.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidCloseTextDocumentParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return OnDidCloseTextDocument(parameters);
        }

        /// <summary>
        ///     Handle a change in document text.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidChangeTextDocumentParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return OnDidChangeTextDocument(parameters);
        }

        /// <summary>
        ///     Handle a document being saved.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public Task Handle(DidSaveTextDocumentParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return OnDidSaveTextDocument(parameters);
        }

        /// <summary>
        ///     Get global registration options for handling document events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions() => RegistrationOptions;

        /// <summary>
        ///     Get registration options for handling document-change events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions() => DocumentChangeRegistrationOptions;

        /// <summary>
        ///     Get registration options for handling document save events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions() => DocumentSaveRegistrationOptions;
        
        /// <summary>
        ///     Called to inform the handler of the language server's document-synchronisation capabilities.
        /// </summary>
        /// <param name="capabilities">
        ///     A <see cref="SynchronizationCapability"/> data structure representing the capabilities.
        /// </param>
        void ICapability<SynchronizationCapability>.SetCapability(SynchronizationCapability capabilities)
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
        TextDocumentAttributes ITextDocumentSyncHandler.GetTextDocumentAttributes(Uri documentUri)
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

            _logger.Information(message, propertyValues);

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

            _logger.Warning(message, propertyValues);

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

            _logger.Error(message, propertyValues);

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

            _logger.Error(exception, message, propertyValues);

            // TODO: _server.LogMessage
        }
    }
}
