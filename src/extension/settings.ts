import * as vscode from 'vscode';

/**
 * The current schema version.
 */
export const currentSchemaVersion = 1;

/**
 * Settings for the MSBuild Project Tools extension.
 */
export interface Settings {
    /**
     * The configuration schema version.
     */
    schemaVersion: number;

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
     * Use the classic completion provider instead of the full language service?
     */
    useClassicProvider?: boolean;

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
     * Exclude suggestions for pre-release packages and package versions?
     */
    includePreRelease?: boolean;

    /**
     * Disable automatic warm-up of the NuGet client when opening a project?
     */
    disablePreFetch?: boolean;
}

/**
 * Update the configuration schema to the current version, if required.
 * 
 * @param configuration The current configuration.
 * @param workspaceConfiguration VS Code's global configuration.
 */
export async function upgradeConfigurationSchema(configuration: Settings): Promise<void> {
    if (configuration.schemaVersion === currentSchemaVersion)
        return;

    const legacyConfiguration = <any>configuration;
    const legacyLanguageConfiguration = legacyConfiguration.language;
    if (legacyLanguageConfiguration) {
        configuration.language.useClassicProvider = (legacyLanguageConfiguration.enable === false);
        delete legacyLanguageConfiguration.enable;

        configuration.language.disable = configuration.language.disable || {};
        configuration.language.disable.hover = (legacyLanguageConfiguration.disableHover === true);
        delete legacyLanguageConfiguration.disableHover;

        configuration.logging = configuration.logging || {};
        configuration.logging.level = legacyLanguageConfiguration.logLevel || 'Information';
        delete legacyLanguageConfiguration.logLevel;
        configuration.logging.file = legacyLanguageConfiguration.logFile || '';
        delete legacyLanguageConfiguration.logFile;
        configuration.logging.trace = (legacyLanguageConfiguration.trace === true);
        delete legacyLanguageConfiguration.trace;

        const legacySeqLoggingConfiguration = legacyLanguageConfiguration.seqLogging;
        if (legacySeqLoggingConfiguration) {
            configuration.logging.seq = configuration.logging.seq || {};
            configuration.logging.seq.level = legacySeqLoggingConfiguration.logLevel || 'Information';
            configuration.logging.seq.url = legacySeqLoggingConfiguration.url || null;
            configuration.logging.seq.apiKey = legacySeqLoggingConfiguration.apiKey || null;

            delete legacyLanguageConfiguration.seqLogging;
        }

        const experimentalFeatures = legacyLanguageConfiguration.experimentalFeatures;
        if (experimentalFeatures) {
            configuration.experimentalFeatures = experimentalFeatures;

            delete legacyLanguageConfiguration.experimentalFeatures;
        }

        // Current schema version.
        configuration.schemaVersion = currentSchemaVersion;

        const workspaceConfiguration = vscode.workspace.getConfiguration();
        await workspaceConfiguration.update('msbuildProjectTools', configuration, true);
    }
}
