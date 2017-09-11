using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace MSBuildProjectTools.LanguageServer
{
    /// <summary>
    ///     The configuration for the MSBuild language service.
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
        ///     Disable the language service?
        /// </summary>
        [JsonProperty("disable")]
        public bool DisableLanguageService { get; set; } = false;

        /// <summary>
        ///     Disable tooltips when hovering on XML in MSBuild project files?
        /// </summary>
        [JsonProperty("disableHover")]
        public bool DisableHover { get; set; } = false;

        /// <summary>
        ///     The minimum log level for regular logging.
        /// </summary>
        [JsonProperty("logLevel")]
        public LogEventLevel LogLevel { get => LogLevelSwitch.MinimumLevel; set => LogLevelSwitch.MinimumLevel = value; }

        /// <summary>
        ///     The serilog log-level switch for regular logging.
        /// </summary>
        [JsonIgnore]
        public LoggingLevelSwitch LogLevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Information);
        
        /// <summary>
        ///     The MSBuild language service's NuGet configuration.
        /// </summary>
        [JsonProperty("nuget", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public NuGetConfiguration NuGet { get; } = new NuGetConfiguration();

        /// <summary>
        ///     The MSBuild language service's Seq logging configuration.
        /// </summary>
        [JsonProperty("seqLogging", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public SeqLoggingConfiguration Seq { get; } = new SeqLoggingConfiguration();

        /// <summary>
        ///     Experimental features (if any) that are currently enabled?
        /// </summary>
        [JsonProperty("experimentalFeatures", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public HashSet<string> EnableExperimentalFeatures { get; } = new HashSet<string>();
    }

    /// <summary>
    ///     NuGet-related configuration for the language service.
    /// </summary>
    public class NuGetConfiguration
    {
        /// <summary>
        ///     Disable automatic warm-up of the NuGet API client?
        /// </summary>
        [JsonProperty("disablePreFetch")]
        public bool DisablePreFetch { get; set; } = false;

        /// <summary>
        ///     Sort package versions in descending order (i.e. newest versions first)?
        /// </summary>
        [JsonProperty("newestVersionsFirst")]
        public bool ShowNewestVersionsFirst { get; set; } = true;
    }

    /// <summary>
    ///     Seq-related logging configuration for the language service.
    /// </summary>
    public class SeqLoggingConfiguration
    {
        /// <summary>
        ///     The URL of the Seq server (or <c>null</c> to disable logging).
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        ///     An optional API key used to authenticate to Seq.
        /// </summary>
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        /// <summary>
        ///     The minimum log level for regular logging.
        /// </summary>
        [JsonProperty("logLevel")]
        public LogEventLevel LogLevel { get => LogLevelSwitch.MinimumLevel; set => LogLevelSwitch.MinimumLevel = value; }

        /// <summary>
        ///     The serilog log-level switch for logging to Seq.
        /// </summary>
        [JsonIgnore]
        public LoggingLevelSwitch LogLevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Verbose);
    }
}
