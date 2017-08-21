using Serilog;
using Serilog.Configuration;
using System;

namespace MSBuildProjectTools.LanguageServer.Logging
{
    using Handlers;

    /// <summary>
    ///     Extension methods for configuring Serilog to log to a language server.
    /// </summary>
    public static class LoggerConfigurationLanguageServerExtensions
    {
        /// <summary>
        ///     Write log events to the language server logging facility.
        /// </summary>
        /// <param name="loggerSinkConfiguration">
        ///     The logger sink configuration.
        /// </param>
        /// <param name="languageServer">
        ///     The language server to which events will be logged.
        /// </param>
        /// <param name="configuration">
        ///     The language server configuration.
        /// </param>
        /// <returns>
        ///     The logger configuration.
        /// </returns>
        public static LoggerConfiguration LanguageServer(this LoggerSinkConfiguration loggerSinkConfiguration, Lsp.LanguageServer languageServer, Configuration configuration)
        {
            if (loggerSinkConfiguration == null)
                throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            
            if (languageServer == null)
                throw new ArgumentNullException(nameof(languageServer));
            
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return loggerSinkConfiguration.Sink(
                new LanguageServerLoggingSink(languageServer, configuration)
            );
        }
    }
}
