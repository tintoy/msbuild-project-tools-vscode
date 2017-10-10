using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace MSBuildProjectTools.LanguageServer
{
    /// <summary>
    ///     The configuration for the MSBuild language service.
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        ///     The name of the configuration section as passed in messages such as <see cref="CustomProtocol.DidChangeConfigurationObjectParams"/>.
        /// </summary>
        public static readonly string SectionName = "msbuildProjectTools";

        /// <summary>
        ///     Create a new <see cref="Configuration"/>.
        /// </summary>
        public Configuration()
        {
        }

        /// <summary>
        ///     The version of the configuration schema in use.
        /// </summary>
        [JsonProperty("schemaVersion", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int SchemaVersion { get; set; }

        /// <summary>
        ///     The MSBuild language service's logging configuration.
        /// </summary>
        [JsonProperty("logging", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public LoggingConfiguration Logging { get; } = new LoggingConfiguration();

        /// <summary>
        ///     The MSBuild language service's main configuration.
        /// </summary>
        [JsonProperty("language", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public LanguageConfiguration Language { get; } = new LanguageConfiguration();
        
        /// <summary>
        ///     The MSBuild language service's NuGet configuration.
        /// </summary>
        [JsonProperty("nuget", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public NuGetConfiguration NuGet { get; } = new NuGetConfiguration();

        /// <summary>
        ///     Experimental features (if any) that are currently enabled.
        /// </summary>
        [JsonProperty("experimentalFeatures", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public HashSet<string> EnableExperimentalFeatures { get; } = new HashSet<string>();
    }

    /// <summary>
    ///     Logging settings for the MSBuild language service.
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        ///     The minimum log level for regular logging.
        /// </summary>
        [JsonProperty("level")]
        public LogEventLevel Level { get => LevelSwitch.MinimumLevel; set => LevelSwitch.MinimumLevel = value; }

        /// <summary>
        ///     The serilog log-level switch for regular logging.
        /// </summary>
        [JsonIgnore]
        public LoggingLevelSwitch LevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Information);

        /// <summary>
        ///     The name of the file (if any) to which log entries are written.
        /// </summary>
        /// <remarks>
        ///     Included here only for completeness; the client supplies this setting via environment variable.
        /// </remarks>
        [JsonProperty("file")]
        public string LogFile { get; set; }

        /// <summary>
        ///     The MSBuild language service's Seq logging configuration.
        /// </summary>
        [JsonProperty("seq", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public SeqLoggingConfiguration Seq { get; } = new SeqLoggingConfiguration();

        /// <summary>
        ///     Enable verbose tracing of LSP messages between client and server?
        /// </summary>
        [JsonProperty("trace")]
        public bool Trace { get; set; }
    }

    /// <summary>
    ///     Seq-related logging configuration for the language service.
    /// </summary>
    public class SeqLoggingConfiguration
    {
        /// <summary>
        ///     The minimum log level for Seq logging.
        /// </summary>
        [JsonProperty("level")]
        public LogEventLevel Level { get => LevelSwitch.MinimumLevel; set => LevelSwitch.MinimumLevel = value; }

        /// <summary>
        ///     The serilog log-level switch for logging to Seq.
        /// </summary>
        [JsonIgnore]
        public LoggingLevelSwitch LevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Verbose);

        /// <summary>
        ///     The URL of the Seq server (or <c>null</c> to disable logging).
        /// </summary>
        /// <remarks>
        ///     Included here only for completeness; the client supplies this setting via environment variable.
        /// </remarks>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        ///     An optional API key used to authenticate to Seq.
        /// </summary>
        /// <remarks>
        ///     Included here only for completeness; the client supplies this setting via environment variable.
        /// </remarks>
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
    }

    /// <summary>
    ///     The main settings for the MSBuild language service.
    /// </summary>
    public class LanguageConfiguration
    {
        /// <summary>
        ///     Create a new <see cref="LanguageConfiguration"/>.
        /// </summary>
        public LanguageConfiguration()
        {
        }

        /// <summary>
        ///     Disable the language service?
        /// </summary>
        [JsonProperty("useClassicProvider")]
        public bool DisableLanguageService { get; set; } = false;

        /// <summary>
        ///     Language service features (if any) to disable.
        /// </summary>
        [JsonProperty("disable", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public DisabledFeatureConfiguration DisableFeature { get; } = new DisabledFeatureConfiguration();

        /// <summary>
        ///     Types of object from the current project to include when offering completions.
        /// </summary>
        [JsonProperty("completionsFromProject", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public HashSet<CompletionSource> CompletionsFromProject { get; } = new HashSet<CompletionSource>();
    }

    /// <summary>
    ///     Configuration for disabled language-service features.
    /// </summary>
    public class DisabledFeatureConfiguration
    {
        /// <summary>
        ///     Disable tooltips when hovering on XML in MSBuild project files?
        /// </summary>
        [JsonProperty("hover")]
        public bool Hover { get; set; }
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
        ///     Exclude suggestions for pre-release packages and package versions?
        /// </summary>
        [JsonProperty("includePreRelease", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IncludePreRelease { get; set; } = false;

        /// <summary>
        ///     Sort package versions in descending order (i.e. newest versions first)?
        /// </summary>
        [JsonProperty("newestVersionsFirst")]
        public bool ShowNewestVersionsFirst { get; set; } = true;
    }

    /// <summary>
    ///     Represents a data-source for completion.
    /// </summary>
    public enum CompletionSource
    {
        /// <summary>
        ///     Item types.
        /// </summary>
        ItemType,

        /// <summary>
        ///     Item metadata names.
        /// </summary>
        ItemMetadata,

        /// <summary>
        ///     Property names.
        /// </summary>
        Property,

        /// <summary>
        ///     Target names.
        /// </summary>
        Target,

        /// <summary>
        ///     Task metadata (names, parameters, etc).
        /// </summary>
        Task
    }
}
