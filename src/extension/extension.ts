'use strict';

import { default as axios } from 'axios';
import * as path from 'path';
import * as semver from 'semver';
import * as vscode from 'vscode';
import { LanguageClient, ServerOptions, LanguageClientOptions, ErrorAction, CloseAction, RevealOutputChannelOn } from 'vscode-languageclient';
import { Trace } from 'vscode-jsonrpc/lib/main';

import * as dotnet from './utils/dotnet';
import * as executables from './utils/executables';
import { PackageReferenceCompletionProvider, getNuGetV3AutoCompleteEndPoints } from './providers/package-reference-completion';
import { handleBusyNotifications } from './notifications';
import { registerCommands } from './commands';
import { registerInternalCommands } from './internal-commands';
import { Settings, upgradeConfigurationSchema } from './settings';

let configuration: Settings;
let languageClient: LanguageClient;
let statusBarItem: vscode.StatusBarItem;
let outputChannel: vscode.OutputChannel;

const featureFlags = new Set<string>();
const languageServerEnvironment = Object.assign({}, process.env);

const projectDocumentSelector: vscode.DocumentSelector = [
    { language: 'xml', pattern: '**/*.*proj' },
    { language: 'xml', pattern: '**/*.props' },
    { language: 'xml', pattern: '**/*.targets' },
    { language: 'xml', pattern: '**/*.tasks' },
    { language: 'msbuild', pattern: '**/*.*' }
];

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

        const enableLanguageService = !configuration.language.useClassicProvider;
        let couldEnableLanguageService = false;
        if (enableLanguageService) {
            const dotnetVersion = await dotnet.getVersion();
            couldEnableLanguageService = dotnetVersion && semver.gte(dotnetVersion, '2.0.0');
        }

        if (enableLanguageService && couldEnableLanguageService) {
            await createLanguageClient(context);

            context.subscriptions.push(
                handleExpressionAutoClose()
            );

            registerCommands(context, statusBarItem);
            registerInternalCommands(context);
        } else {
            await createClassicCompletionProvider(context, couldEnableLanguageService);

            outputChannel.appendLine('Classic completion provider is now enabled.');
        }
    });

    context.subscriptions.push(
        vscode.workspace.onDidChangeConfiguration(async args => {
            await loadConfiguration();

            if (languageClient) {
                if (configuration.logging.trace) {
                    languageClient.trace = Trace.Verbose;
                } else {
                    languageClient.trace = Trace.Off;
                }
            }
        })
    );
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
    
    await upgradeConfigurationSchema(configuration);

    featureFlags.clear();
    if (configuration.experimentalFeatures) {
        configuration.experimentalFeatures.forEach(
            featureFlag => featureFlags.add(featureFlag)
        );
    }
}

/**
 * Create the classic completion provider for PackageReferences.
 * 
 * @param context The current extension context.
 * @param canEnableLanguageService Could the language service be enabled if we wanted to?
 */
async function createClassicCompletionProvider(context: vscode.ExtensionContext, canEnableLanguageService: boolean): Promise<void> {
    outputChannel = vscode.window.createOutputChannel('MSBuild Project Tools');

    if (!configuration.language.useClassicProvider && !canEnableLanguageService)
        outputChannel.appendLine('Cannot enable the MSBuild language service because .NET Core >= 2.0.0 was not found on the system path.');

    outputChannel.appendLine('MSBuild language service disabled; using the classic completion provider.');

    const nugetEndPointURLs = await getNuGetV3AutoCompleteEndPoints();
    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(projectDocumentSelector,
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
        synchronize: {
            configurationSection: 'msbuildProjectTools'
        },
        diagnosticCollectionName: 'MSBuild Project',
        errorHandler: {
            error: (error, message, count) => {
                console.log(message);
                console.log(error);

                return ErrorAction.Continue;
            },
            closed: () => CloseAction.Restart
        },
        revealOutputChannelOn: RevealOutputChannelOn.Never
    };

    languageServerEnvironment['MSBUILD_PROJECT_TOOLS_DIR'] = context.extensionPath;

    const seqLoggingSettings = configuration.logging.seq;
    if (seqLoggingSettings && seqLoggingSettings.url) {
        languageServerEnvironment['MSBUILD_PROJECT_TOOLS_SEQ_URL'] = seqLoggingSettings.url;
        languageServerEnvironment['MSBUILD_PROJECT_TOOLS_SEQ_API_KEY'] = seqLoggingSettings.apiKey;
    }

    if (configuration.logging.file) {
        languageServerEnvironment['MSBUILD_PROJECT_TOOLS_LOG_FILE'] = configuration.logging.file;
    }

    const dotNetExecutable = await executables.find('dotnet');
    const serverAssembly = context.asAbsolutePath('out/language-server/MSBuildProjectTools.LanguageServer.Host.dll');
    const serverOptions: ServerOptions = {
        command: dotNetExecutable,
        args: [serverAssembly],
        options: {
            env: languageServerEnvironment
        }
    };

    languageClient = new LanguageClient('MSBuild Project Tools', serverOptions, clientOptions);
    if (configuration.logging.trace) {
        languageClient.trace = Trace.Verbose;
    } else {
        languageClient.trace = Trace.Off;
    }

    handleBusyNotifications(languageClient, statusBarItem);

    outputChannel = languageClient.outputChannel;
    outputChannel.appendLine('Starting MSBuild language service...');
    context.subscriptions.push(
        languageClient.start()
    );
    await languageClient.onReady();
    outputChannel.appendLine('MSBuild language service is running.');
}

/**
 * Handle document-change events to automatically insert a closing parenthesis for common MSBuild expressions.
 */
function handleExpressionAutoClose(): vscode.Disposable {
    return vscode.workspace.onDidChangeTextDocument(async args => {
        if (!vscode.languages.match(projectDocumentSelector, args.document))
            return;

        if (!featureFlags.has('expressions'))
            return;

        if (args.contentChanges.length !== 1)
            return; // Completion doesn't make sense with multiple cursors.

        const contentChange = args.contentChanges[0];
        if (isOriginPosition(contentChange.range.start))
            return; // We're at the start of the document; no previous character to check.

        if (contentChange.text === '(') {
            // Select the previous character and the one they just typed.
            const range = contentChange.range.with(
                contentChange.range.start.translate(0, -1),
                contentChange.range.end.translate(0, 1)
            );

            const openExpression = args.document.getText(range);
            switch (openExpression) {
                case '$(': // Eval open
                case '@(': // Item group open
                case '%(': // Item metadata open
                    {
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            // Replace open expression with a closed one.
            const closedExpression = openExpression + ')';
            await vscode.window.activeTextEditor.edit(
                edit => edit.replace(range, closedExpression)
            );

            // Move between the parentheses and trigger completion.
            await vscode.commands.executeCommand('msbuildProjectTools.internal.moveAndSuggest',
                'left',      // moveTo
                'character', // moveBy
                1            // moveCount
            );
        }
    });
}

/**
 * Determine whether the specified {@link vscode.Position} represents the origin position.
 * 
 * @param position The {@link vscode.Position} to examine.
 */
function isOriginPosition(position: vscode.Position): boolean {
    return position.line === 0 && position.character === 0;
}
