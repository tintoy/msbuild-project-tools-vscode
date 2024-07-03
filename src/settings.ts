import * as objectpath from 'object-path';
import * as vscode from 'vscode';

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

/**
 * Update the configuration schema to the current version, if required.
 * 
 * @param configuration The current configuration.
 * @param workspaceConfiguration VS Code's global configuration.
 * @returns The updated configuration
 */
export async function upgradeConfigurationSchema(configuration: any): Promise<any> {
    if (!configuration.schemaVersion)
        return;

    let modified = false;

    const workspaceConfiguration = vscode.workspace.getConfiguration();

    const legacyExperimentalFeatureConfiguration = configuration.experimentalFeatures;
    if (legacyExperimentalFeatureConfiguration) {
        await workspaceConfiguration.update('msbuildProjectTools.experimentalFeatures',
            legacyExperimentalFeatureConfiguration || [],
            true // global
        );

        modified = true;
    }

    const legacyLanguageConfiguration = configuration.language;
    if (legacyLanguageConfiguration) {
        if (legacyLanguageConfiguration.disableHover) {
            await workspaceConfiguration.update('msbuildProjectTools.language.disableHover',
                legacyLanguageConfiguration.disableHover,
                true // global
            );

            modified = true;
        }
    }

    const legacyNugetConfiguration = configuration.nuget as NuGetSettings;
    if (legacyNugetConfiguration) {
        if (legacyNugetConfiguration.disablePreFetch) {
            await workspaceConfiguration.update('msbuildProjectTools.nuget.disablePreFetch',
            legacyNugetConfiguration.disablePreFetch,
            true // global
        );

            modified = true;
        }
        if (legacyNugetConfiguration.includePreRelease) {
            await workspaceConfiguration.update('msbuildProjectTools.nuget.includePreRelease',
                legacyNugetConfiguration.includePreRelease,
                true // global
            );

            modified = true;
        }
        if (legacyNugetConfiguration.newestVersionsFirst) {
            await workspaceConfiguration.update('msbuildProjectTools.nuget.newestVersionsFirst',
                legacyNugetConfiguration.newestVersionsFirst,
                true // global
            );

            modified = true;
        }
    }

    const legacyLoggingConfiguration = configuration.logging;
    if (legacyLoggingConfiguration) {
        if (legacyLoggingConfiguration.logLevel) {
            await workspaceConfiguration.update('msbuildProjectTools.nuget.newestVersionsFirst',
                legacyLoggingConfiguration.logLevel,
                true // global
            );

            modified = true;
        }
        if (legacyLoggingConfiguration.file) {
            await workspaceConfiguration.update('msbuildProjectTools.nuget.newestVersionsFirst',
                legacyLoggingConfiguration.file,
                true // global
            );

            modified = true;
        }
        if (legacyLoggingConfiguration.trace) {
            await workspaceConfiguration.update('msbuildProjectTools.nuget.newestVersionsFirst',
                legacyLoggingConfiguration.trace,
                true // global
            );

            modified = true;
        }

        const legacySeqLoggingConfiguration = legacyLoggingConfiguration.seq;
        if (legacySeqLoggingConfiguration) {
            if (legacySeqLoggingConfiguration.level) {
                await workspaceConfiguration.update('msbuildProjectTools.logging.seq.level',
                    legacySeqLoggingConfiguration.level,
                    true // global
                );

                modified = true;
            }
            if (legacySeqLoggingConfiguration.url) {
                await workspaceConfiguration.update('msbuildProjectTools.logging.seq.url',
                    legacySeqLoggingConfiguration.url,
                    true // global
                );

                modified = true;
            }
            if (legacySeqLoggingConfiguration.apiKey) {
                await workspaceConfiguration.update('msbuildProjectTools.logging.seq.apiKey',
                    legacySeqLoggingConfiguration.apiKey,
                    true // global
                );

                modified = true;
            }
        }

        configuration.logging = null;

        modified = true;
    }

    if (configuration.schemaVersion) {
        configuration.schemaVersion = null;

        modified = true;
    }

    if (modified) {
        // TODO: Show notification indicating that old settings key should be deleted.
        await vscode.window.showInformationMessage('MSBuild project tools settings have been upgraded to the latest version; please manually remove the old key ("msbuildProjectTools") from settings.json.');
    }
}
