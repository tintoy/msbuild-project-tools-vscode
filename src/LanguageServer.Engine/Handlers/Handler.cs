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

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    /// <summary>
    ///     The base class for language server event handlers.
    /// </summary>
    public abstract class Handler
    {
        /// <summary>
        ///     Create a new <see cref="Handler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        protected Handler(ILanguageServer server, ILogger logger)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));

            Server = server;
            Log = logger.ForContext(GetType());
        }

        /// <summary>
        ///     The handler's logger.
        /// </summary>
        protected ILogger Log { get; }

        /// <summary>
        ///     The language server.
        /// </summary>
        protected ILanguageServer Server { get; }
    }
}
