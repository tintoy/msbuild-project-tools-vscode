using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.IO;
using JsonRpc;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    /// <summary>
    ///     The base class for language server text-document event handlers.
    /// </summary>
    public abstract class TextDocumentHandler
        : Handler, ICompletionHandler, IDocumentSymbolHandler, IDefinitionHandler
    {
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
            : base(server, logger)
        {
        }

        /// <summary>
        ///     The document selector that describes documents targeted by the handler.
        /// </summary>
        protected abstract DocumentSelector DocumentSelector { get; }

        /// <summary>
        ///     The server's completion capabilities.
        /// </summary>
        protected CompletionCapability CompletionCapabilities { get; private set; }

        /// <summary>
        ///     The server's document symbol capabilities.
        /// </summary>
        protected DocumentSymbolCapability DocumentSymbolCapabilities { get; private set; }

        /// <summary>
        ///     The server's definition capabilities.
        /// </summary>
        protected DefinitionCapability DefinitionCapabilities { get; private set; }

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
        protected virtual Task<CompletionList> OnCompletion(TextDocumentPositionParams parameters, CancellationToken cancellationToken) => Task.FromResult<CompletionList>(null);

        /// <summary>
        ///     Called when document symbols are requested.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the symbol container or <c>null</c> if no symbols are provided.
        /// </returns>
        protected virtual Task<SymbolInformationContainer> OnDocumentSymbols(DocumentSymbolParams parameters, CancellationToken cancellationToken) => Task.FromResult<SymbolInformationContainer>(null);

        /// <summary>
        ///     Called when a definition is requested.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the definition location or <c>null</c> if no definition is provided.
        /// </returns>
        protected virtual Task<LocationOrLocations> OnDefinition(TextDocumentPositionParams parameters, CancellationToken cancellationToken) => Task.FromResult<LocationOrLocations>(null);

        /// <summary>
        ///     Get global registration options for handling document events.
        /// </summary>
        protected virtual TextDocumentRegistrationOptions DocumentRegistrationOptions
        {
            get => new TextDocumentRegistrationOptions
            {
                DocumentSelector = DocumentSelector
            };
        }

        /// <summary>
        ///     Get registration options for handling completion requests events.
        /// </summary>
        protected virtual CompletionRegistrationOptions CompletionRegistrationOptions
        {
            get => new CompletionRegistrationOptions
            {
                DocumentSelector = DocumentSelector,
                ResolveProvider = false
            };
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
        protected virtual TextDocumentAttributes GetTextDocumentAttributes(Uri documentUri) => new TextDocumentAttributes(documentUri, "xml");

        /// <summary>
        ///     Handle a request for completion.
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
        async Task<CompletionList> IRequestHandler<TextDocumentPositionParams, CompletionList>.Handle(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            try
            {
                return await OnCompletion(parameters, cancellationToken);
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unhandled exception in {Method:l}.", "OnCompletion");

                return null;
            }
        }

        /// <summary>
        ///     Handle a request for document symbols.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the symbol container or <c>null</c> if no symbols are provided.
        /// </returns>
        async Task<SymbolInformationContainer> IRequestHandler<DocumentSymbolParams, SymbolInformationContainer>.Handle(DocumentSymbolParams parameters, CancellationToken cancellationToken)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            try
            {
                return await OnDocumentSymbols(parameters, cancellationToken);
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unhandled exception in {Method:l}.", "OnDocumentSymbols");

                return null;
            }
        }

        /// <summary>
        ///     Handle a request for a definition.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is definition location or <c>null</c> if no definition is provided.
        /// </returns>
        async Task<LocationOrLocations> IRequestHandler<TextDocumentPositionParams, LocationOrLocations>.Handle(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            try
            {
                return await OnDefinition(parameters, cancellationToken);
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unhandled exception in {Method:l}.", "OnDefinition");

                return null;
            }
        }

        /// <summary>
        ///     Get global registration options for handling document events.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions() => DocumentRegistrationOptions;

        /// <summary>
        ///     Get registration options for handling completion requests.
        /// </summary>
        /// <returns>
        ///     The registration options.
        /// </returns>
        CompletionRegistrationOptions IRegistration<CompletionRegistrationOptions>.GetRegistrationOptions() => CompletionRegistrationOptions;

        /// <summary>
        ///     Called to inform the handler of the language server's completion capabilities.
        /// </summary>
        /// <param name="capabilities">
        ///     A <see cref="CompletionCapability"/> data structure representing the capabilities.
        /// </param>
        void ICapability<CompletionCapability>.SetCapability(CompletionCapability capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            CompletionCapabilities = capabilities;
        }

        /// <summary>
        ///     Called to inform the handler of the language server's document symbol capabilities.
        /// </summary>
        /// <param name="capabilities">
        ///     A <see cref="DocumentSymbolCapability"/> data structure representing the capabilities.
        /// </param>
        void ICapability<DocumentSymbolCapability>.SetCapability(DocumentSymbolCapability capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            DocumentSymbolCapabilities = capabilities;
        }

        /// <summary>
        ///     Called to inform the handler of the language server's definition capabilities.
        /// </summary>
        /// <param name="capabilities">
        ///     A <see cref="DefinitionCapability"/> data structure representing the capabilities.
        /// </param>
        void ICapability<DefinitionCapability>.SetCapability(DefinitionCapability capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));
            
            DefinitionCapabilities = capabilities;
        }
    }
}
