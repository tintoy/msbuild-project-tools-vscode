using Serilog;
using Serilog.Configuration;
using Serilog.Core;
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
        /// <param name="levelSwitch">
        ///     The <see cref="LoggingLevelSwitch"/> that controls logging.
        /// </param>
        /// <returns>
        ///     The logger configuration.
        /// </returns>
        public static LoggerConfiguration LanguageServer(this LoggerSinkConfiguration loggerSinkConfiguration, Lsp.LanguageServer languageServer, LoggingLevelSwitch levelSwitch)
        {
            if (loggerSinkConfiguration == null)
                throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            
            if (languageServer == null)
                throw new ArgumentNullException(nameof(languageServer));
            
            if (levelSwitch == null)
                throw new ArgumentNullException(nameof(levelSwitch));

            return loggerSinkConfiguration.Sink(
                new LanguageServerLoggingSink(languageServer, levelSwitch)
            );
        }
    }
}
