using OmniSharp.Extensions.LanguageServer;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using System;

namespace MSBuildProjectTools.LanguageServer.Logging
{
    using Handlers;

    /// <summary>
    ///     Extension methods for configuring Serilog.
    /// </summary>
    public static class SerilogLanguageServerExtensions
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
        public static LoggerConfiguration LanguageServer(this LoggerSinkConfiguration loggerSinkConfiguration, ILanguageServer languageServer, LoggingLevelSwitch levelSwitch)
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

        /// <summary>
        ///     Enrich log events with the current logical activity Id (if any).
        /// </summary>
        /// <param name="loggerEnrichmentConfiguration">
        ///     The logger enrichment configuration.
        /// </param>
        /// <returns>
        ///     The logger configuration.
        /// </returns>
        public static LoggerConfiguration WithCurrentActivityId(this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration)
        {
            if (loggerEnrichmentConfiguration == null)
                throw new ArgumentNullException(nameof(loggerEnrichmentConfiguration));
            
            return loggerEnrichmentConfiguration.With<ActivityIdEnricher>();
        }
    }
}
