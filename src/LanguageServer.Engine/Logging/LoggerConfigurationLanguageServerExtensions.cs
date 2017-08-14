using Serilog;
using Serilog.Configuration;

namespace MSBuildProjectTools.LanguageServer.Logging
{
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
        /// <returns>
        ///     The logger configuration.
        /// </returns>
        public static LoggerConfiguration LanguageServer(this LoggerSinkConfiguration loggerSinkConfiguration, Lsp.LanguageServer languageServer)
        {
            return loggerSinkConfiguration.Sink(
                new LanguageServerLoggingSink(languageServer)
            );
        }
    }
}
