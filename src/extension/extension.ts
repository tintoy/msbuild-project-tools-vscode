'use strict';

import { default as axios } from 'axios';
import * as path from 'path';
import * as semver from 'semver';
import * as vscode from 'vscode';
import { LanguageClient, ServerOptions, LanguageClientOptions, ErrorAction, CloseAction } from 'vscode-languageclient';

import * as dotnet from './utils/dotnet';
import * as executables from './utils/executables';
import { PackageReferenceCompletionProvider, getNuGetV3AutoCompleteEndPoints } from './providers/package-reference-completion';
import { handleBusyNotifications } from './notifications';

/**
 * Enable the MSBuild language service?
 */
let configuration: Settings;
let legacySettingsPresent = false;
let statusBarItem: vscode.StatusBarItem;
let outputChannel: vscode.OutputChannel;

/**
 * Called when the extension is activated.
 * 
 * @param context The extension context.
 */
export async function activate(context: vscode.ExtensionContext): Promise<void> {
    const progressOptions: vscode.ProgressOptions = {
        location: vscode.ProgressLocation.Window
    };
    await vscode.window.withProgress(progressOptions, async progress => {
        progress.report({
            message: 'Initialising MSBuild project tools...'
        });

        await loadConfiguration();
        let couldEnableLanguageService = false;
        if (configuration.language.enable) {
            const dotnetVersion = await dotnet.getVersion();
            couldEnableLanguageService = dotnetVersion && semver.gte(dotnetVersion, '2.0.0');
        }
    
        if (configuration.language.enable && couldEnableLanguageService) {
            await createLanguageClient(context);
        } else {
            await createClassicCompletionProvider(context, couldEnableLanguageService);
    
            outputChannel.appendLine('Classic completion provider is now enabled.');
        }

        if (legacySettingsPresent) {
            outputChannel.appendLine(
                'Warning: legacy settings detected.'
            );
            outputChannel.appendLine(
                `The "msbuildProjectFileTools" section in settings is now called "msbuildProjectTools"; these legacy settings will override the extension's current settings until you remove them.`
            );
        }
    });
}

/**
 * Called when the extension is deactivated.
 */
export function deactivate(): void {
    // Nothing to clean up.
}

/**
 * Load extension configuration from the workspace.
 */
async function loadConfiguration(): Promise<void> {
    const workspaceConfiguration = vscode.workspace.getConfiguration();

    configuration = workspaceConfiguration.get<Settings>('msbuildProjectTools');
    
    // Use settings in old format, if present.
    const legacyConfiguration = workspaceConfiguration.msbuildProjectFileTools;
    if (legacyConfiguration) {
        legacySettingsPresent = true;

        configuration.language.enable = legacyConfiguration.languageService.enable || true;
        configuration.language.disableHover = legacyConfiguration.languageService.disableHover || false;
        await workspaceConfiguration.update('msbuildProjectTools', configuration, true);
    }
}

/**
 * Settings for the MSBuild Project Tools extension.
 */
interface Settings {
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
interface LanguageSettings {
    /**
     * Enable the language service?
     */
    enable: boolean;

    /**
     * Disable tooltips when hovering over XML in MSBuild project files.
     */
    disableHover: boolean;
}

/**
 * NuGet settings.
 */
interface NuGetSettings {
    /**
     * Sort package versions in descending order (i.e. newest versions first)?
     */
    newestVersionsFirst: boolean;

    /**
     * Disable automatic warm-up of the NuGet client when opening a project?
     */
    disablePreFetch: boolean;
}

/**
 * Create the classic completion provider for PackageReferences.
 * 
 * @param context The current extension context.
 * @param canEnableLanguageService Could the language service be enabled if we wanted to?
 */
async function createClassicCompletionProvider(context: vscode.ExtensionContext, canEnableLanguageService: boolean): Promise<void> {
    outputChannel = vscode.window.createOutputChannel('MSBuild Project Tools');
    
    if (configuration.language.enable && !canEnableLanguageService)
        outputChannel.appendLine('Cannot enable the MSBuild language service because .NET Core >= 2.0.0 was not found on the system path.');

    outputChannel.appendLine('MSBuild language service disabled; using the classic completion provider.');

    const nugetEndPointURLs = await getNuGetV3AutoCompleteEndPoints();
    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(
            [
                { language: 'xml', pattern: '**/*.*proj' },
                { language: 'msbuild', pattern: '**/*.*' }
            ], 
            new PackageReferenceCompletionProvider(
                nugetEndPointURLs[0], // For now, just default to using the primary.
                configuration.nuget.newestVersionsFirst
            )
        )
    );
}

/**
 * Create the MSBuild language client.
 * 
 * @param context The current extension context.
 * @returns A promise that resolves to the language client.
 */
async function createLanguageClient(context: vscode.ExtensionContext): Promise<void> {
    statusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right, 50);
    context.subscriptions.push(statusBarItem);

    statusBarItem.text = '$(check) MSBuild Project';
    statusBarItem.tooltip = 'MSBuild Project Tools';
    statusBarItem.hide();

    const clientOptions: LanguageClientOptions = {
        documentSelector: [{
            language: 'xml',
            pattern: '*.csproj'
        }],
        synchronize: {
            configurationSection: 'msbuildProjectTools'
        },
        errorHandler: {
            error: (error, message, count) =>
            {
                console.log(message);
                console.log(error);

                return ErrorAction.Continue;
            },
            closed: () => CloseAction.Restart
        }
    };

    const dotNetExecutable = await executables.find('dotnet');
    const serverAssembly = context.asAbsolutePath('out/language-server/LanguageServer.dll');
    const serverOptions: ServerOptions = {
        command: dotNetExecutable,
        args: [ serverAssembly ],
    };

    let languageClient = new LanguageClient('MSBuild Project Tools', serverOptions, clientOptions);
    handleBusyNotifications(languageClient, statusBarItem);

    outputChannel = languageClient.outputChannel;
    outputChannel.appendLine('Starting MSBuild language service...');
    context.subscriptions.push(
        languageClient.start()
    );
    await languageClient.onReady();
    outputChannel.appendLine('MSBuild language service is running.');
}
