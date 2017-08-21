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
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

        /// <summary>
        ///     Disable tooltips when hovering on XML in MSBuild project files?
        /// </summary>
        public bool DisableHover { get; set; } = false;
    }
}
