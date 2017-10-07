/**
 * Settings for the MSBuild Project Tools extension.
 */
export interface Settings {
    /**
     * Language service settings.
     */
    language: LanguageSettings;

    /**
     * NuGet settings.
     */
    nuget: NuGetSettings;
}

/**
 * Language service settings.
 */
export interface LanguageSettings {
    /**
     * Enable the language service?
     */
    enable: boolean;

    /**
     * The log file (if any) that the language service should log to.
     */
    logFile?: string;

    /**
     * Enable verbose tracing of messages between the language client and language service?
     */
    trace: boolean;

    /**
     * Disable tooltips when hovering over XML in MSBuild project files.
     */
    disableHover: boolean;

    /**
     * Seq logging settings.
     */
    seqLogging?: SeqLoggingSettings;

    /**
     * Experimental feature flags.
     */
    experimentalFeatures?: string[];
}

/**
 * Seq logging settings.
 */
export interface SeqLoggingSettings {
    /**
     * The Seq server URL (null or empty to disable Seq logging).
     */
    url: string | null;

    /**
     * The Seq API key (if any).
     */
    apiKey?: string;
}

/**
 * NuGet settings.
 */
export interface NuGetSettings {
    /**
     * Sort package versions in descending order (i.e. newest versions first)?
     */
    newestVersionsFirst: boolean;

    /**
     * Exclude suggestions for pre-release packages and package versions?
     */
    includePreRelease: boolean;

    /**
     * Disable automatic warm-up of the NuGet client when opening a project?
     */
    disablePreFetch: boolean;
}
