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
        : Handler, IDefinitionHandler
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
        ///     The server's definition capabilities.
        /// </summary>
        protected DefinitionCapability DefinitionCapabilities { get; private set; }

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
