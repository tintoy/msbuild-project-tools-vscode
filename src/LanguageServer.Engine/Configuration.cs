using Serilog.Core;
using Serilog.Events;

namespace MSBuildProjectTools.LanguageServer
{
    /// <summary>
    ///     The configuration for the language server.
    /// /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        ///     Create a new <see cref="Configuration"/>.
        /// </summary>
        public Configuration()
        {
        }

        /// <summary>
        ///     The currently-configured minimum log level.
        /// </summary>
        public LogEventLevel LogLevel
        {
            get => LogLevelSwitch.MinimumLevel;
            set => LogLevelSwitch.MinimumLevel = value;
        }

        /// <summary>
        ///     The currently-configured minimum log level for logging to Seq.
        /// </summary>
        public LogEventLevel SeqLogLevel
        {
            get => SeqLogLevelSwitch.MinimumLevel;
            set => SeqLogLevelSwitch.MinimumLevel = value;
        }

        /// <summary>
        ///     The serilog log-level switch for regular logging.
        /// </summary>
        public LoggingLevelSwitch LogLevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Information);

        /// <summary>
        ///     The serilog log-level switch for logging to Seq.
        /// </summary>
        public LoggingLevelSwitch SeqLogLevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Verbose);

        /// <summary>
        ///     Disable tooltips when hovering on XML in MSBuild project files?
        /// </summary>
        public bool DisableHover { get; set; } = false;

        /// <summary>
        ///     Disable automatic warm-up of the NuGet API client?
        /// </summary>
        public bool DisableNuGetPreFetch { get; set; } = false;

        /// <summary>
        ///     Sort package versions in descending order (i.e. newest versions first)?
        /// </summary>
        public bool ShowNewestNuGetVersionsFirst { get; set; } = true;
    }
}
