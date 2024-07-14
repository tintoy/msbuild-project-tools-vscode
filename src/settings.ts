import * as objectpath from 'object-path';

// TODO: Use dotted setting names to populated nested structure copied from defaultSettings.

export const defaultSettings: Readonly<Settings> = {
    logging: {
        seq: {},
        level: 'Information'
    },
    language: {
        disable: {}
    },
    nuget: {},
    experimentalFeatures: []
};

/**
 * VS Code's settings (supplied as a dict, essentially).
 */
export interface VSCodeSettings {
    [key: string]: any;
}

/**
 * Read and parse a VSCode-style settings object.
 * 
 * @param vscodeSettings The settings object (keys are 'msbuildProjectTools.xxx').
 * 
 * @returns The parsed settings.
 */
export function readVSCodeSettings(vscodeSettings: VSCodeSettings): Settings {
    const settings: Settings = Object.assign({}, defaultSettings);
    const settingsHelper = objectpath(settings);

    for (const key of Object.getOwnPropertyNames(vscodeSettings)) {
        const value = vscodeSettings[key];

        const path = key.replace('msbuildProjectTools.', '');
        settingsHelper.set(path, value);
    }

    return settings;
}

/**
 * Settings for the MSBuild Project Tools extension.
 */
export interface Settings {
    /**
     * Logging settings.
     */
    logging: LoggingSettings;

    /**
     * Language service settings.
     */
    language: LanguageSettings;

    /**
     * NuGet settings.
     */
    nuget: NuGetSettings;

    /**
     * Experimental feature flags.
     */
    experimentalFeatures?: string[];
}

/**
 * Logging-related settings.
 */
export interface LoggingSettings {
    /**
     * The minimum level to log at.
     */
    level?: string;
    
    /**
     * The log file (if any) that the language service should log to.
     */
    file?: string;
    
    /**
     * Enable verbose tracing of messages between the language client and language service?
     */
    trace?: boolean;
    
    /**
     * Seq-related logging settings.
     */
    seq?: SeqLoggingSettings;
}

/**
 * Seq logging settings.
 */
export interface SeqLoggingSettings {
    /**
     * The minimum level to log to Seq at.
     */
    level?: string;

    /**
     * The Seq server URL (null or empty to disable Seq logging).
     */
    url?: string | null;

    /**
     * The Seq API key (if any).
     */
    apiKey?: string;
}

/**
 * Language service settings.
 */
export interface LanguageSettings {
    /**
     * Disabled language-service features.
     */
    disable: DisabledFeatureSettings;
}

/**
 * Settings for disabled language-service features.
 */
export interface DisabledFeatureSettings {
    /**
     * Disable tooltips when hovering over XML in MSBuild project files?
     */
    hover?: boolean;
}

/**
 * NuGet settings.
 */
export interface NuGetSettings {
    /**
     * Sort package versions in descending order (i.e. newest versions first)?
     */
    newestVersionsFirst?: boolean;

    /**
     * Include suggestions for pre-release packages and package versions?
     */
    includePreRelease?: boolean;

    /**
     * Include suggestions for packages from local (file-based) package sources?
     */
    includeLocalSources?: boolean;

    /**
     * Disable automatic warm-up of the NuGet client when opening a project?
     */
    disablePreFetch?: boolean;
}
