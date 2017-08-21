using Lsp;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;
    
namespace MSBuildProjectTools.LanguageServer.Logging
{
    using Handlers;

    /// <summary>
    ///     Language Server message handler that tracks logging configuration.
    /// </summary>
    sealed class LoggingConfigurationHandler
        : Handler
    {
        /// <summary>
        ///     Create a new <see cref="LoggingConfigurationHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server that hosts the handler.
        /// </param>
        public LoggingConfigurationHandler(ILanguageServer server)
            : base(server, Serilog.Log.Logger)
        {
        }

        /// <summary>
        ///     The currently-configured minimum log level.
        /// </summary>
        public LogEventLevel LogLevel { get; private set; } = LogEventLevel.Information;

        /// <summary>
        ///     Called when configuration has changed.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override Task OnDidChangeConfiguration(Lsp.Models.DidChangeConfigurationParams parameters)
        {
            Lsp.Models.BooleanNumberString logLevelValue;
            if (!parameters.Settings.TryGetValue("logLevel", out logLevelValue) || !logLevelValue.IsString)
                return Task.CompletedTask;

            LogEventLevel configuredLogLevel;
            if (!Enum.TryParse(logLevelValue.String, true, out configuredLogLevel))
                configuredLogLevel = LogEventLevel.Information;
            
            LogLevel = configuredLogLevel;

            return Task.CompletedTask;
        }
    }
}
