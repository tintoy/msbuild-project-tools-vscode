using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer;
using OmniSharp.Extensions.LanguageServer.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer.Tests.Stubs
{
    /// <summary>
    ///     A stub implementation of LSP's <see cref="ILanguageServer"/>.
    /// </summary>
    /// <remarks>
    ///     Can be used as a base class for more full-featured implementations.
    /// </remarks>
    public class StubLanguageServer
        : ILanguageServer
    {
        /// <summary>
        ///     Create a new <see cref="StubLanguageServer"/>.
        /// </summary>
        public StubLanguageServer()
        {
        }

        /// <summary>
        ///     The initialisation parameters sent by the client.
        /// </summary>
        public virtual InitializeParams Client { get; } = new InitializeParams();

        /// <summary>
        ///     The initialisation result returned by the server.
        /// </summary>
        public virtual InitializeResult Server { get; } = new InitializeResult();

        /// <summary>
        ///     Add a handler to the server's collection of JSON RPC handlers.
        /// </summary>
        /// <param name="handler">
        ///     The handler to add.
        /// </param>
        /// <returns>
        ///     An <see cref="IDisposable"/> representing the handler; when disposed, the handler is removed from the handler collection.
        /// </returns>
        public virtual IDisposable AddHandler(IJsonRpcHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return Disposable.Empty;
        }

        /// <summary>
        ///     Add handlers to the server's collection of JSON RPC handlers.
        /// </summary>
        /// <param name="handlers">
        ///     The handlers to add.
        /// </param>
        /// <returns>
        ///     An <see cref="IDisposable"/> representing the handlers; when disposed, the handlers are removed from the handler collection.
        /// </returns>
        public virtual IDisposable AddHandlers(IEnumerable<IJsonRpcHandler> handlers)
        {
            if (handlers == null)
                throw new ArgumentNullException(nameof(handlers));
            
            return Disposable.Empty;
        }

        /// <summary>
        ///     Add handlers to the server's collection of JSON RPC handlers.
        /// </summary>
        /// <param name="handlers">
        ///     The handlers to add.
        /// </param>
        /// <returns>
        ///     An <see cref="IDisposable"/> representing the handlers; when disposed, the handlers are removed from the handler collection.
        /// </returns>
        public IDisposable AddHandlers(params IJsonRpcHandler[] handlers)
        {
            if (handlers == null)
                throw new ArgumentNullException(nameof(handlers));
            
            return Disposable.Empty;
        }

        /// <summary>
        ///     Retrieve the completion source for the request with the specified Id.
        /// </summary>
        /// <param name="id">
        ///     The request Id.
        /// </param>
        /// <returns>
        ///     The <see cref="TaskCompletionSource{TResult}"/>, or <c>null</c> if no outstanding request was found with the specified Id.
        /// </returns>
        public virtual TaskCompletionSource<JToken> GetRequest(long id)
        {
            return null;
        }

        /// <summary>
        ///     Send a notification to the client.
        /// </summary>
        /// <typeparam name="TNotification">
        ///     The notification payload data-type.
        /// </typeparam>
        /// <param name="method">
        ///     The notification method name.
        /// </param>
        /// <param name="notification">
        ///     The notification payload.
        /// </param>
        public virtual void SendNotification<TNotification>(string method, TNotification notification)
        {
            if (String.IsNullOrWhiteSpace(method))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'method'.", nameof(method));
        }

        /// <summary>
        ///     Send a request to the client.
        /// </summary>
        /// <typeparam name="TRequest">
        ///     The request payload daya-type.
        /// </typeparam>
        /// <typeparam name="TResponse">
        ///     The response payload data-type.
        /// </typeparam>
        /// <param name="method">
        ///     The request method name.
        /// </param>
        /// <param name="request">
        ///     The request payload.
        /// </param>
        /// <returns>
        ///     The client's response.
        /// </returns>
        public virtual Task<TResponse> SendRequest<TRequest, TResponse>(string method, TRequest request)
        {
            if (String.IsNullOrWhiteSpace(method))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'method'.", nameof(method));

            return Task.FromResult(
                default(TResponse)
            );
        }

        /// <summary>
        ///     Send a request to the client.
        /// </summary>
        /// <typeparam name="TRequest">
        ///     The request payload daya-type.
        /// </typeparam>
        /// <param name="method">
        ///     The request method name.
        /// </param>
        /// <param name="request">
        ///     The request payload.
        /// </param>
        public virtual Task SendRequest<TRequest>(string method, TRequest request)
        {
            return Task.CompletedTask;
        }
    }
}
