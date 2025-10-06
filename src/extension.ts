'use strict';

import { realpathSync } from 'fs';
import * as vscode from 'vscode';
import { LanguageClientOptions, ErrorAction, CloseAction, RevealOutputChannelOn } from 'vscode-languageclient';
import { LanguageClient, ServerOptions } from 'vscode-languageclient/lib/node/main';
import { Trace } from 'vscode-jsonrpc/lib/node/main';

import * as dotnet from './dotnet';
import { handleBusyNotifications } from './notifications';
import { registerInternalCommands } from './internal-commands';

let languageClient: LanguageClient;
let statusBarItem: vscode.StatusBarItem;
let outputChannel: vscode.OutputChannel;

const languageServerEnvironment = Object.assign({}, process.env);

const projectDocumentSelector: vscode.DocumentSelector = [
    { language: 'xml', pattern: '**/*.*proj' },
    { language: 'xml', pattern: '**/*.props' },
    { language: 'xml', pattern: '**/*.targets' },
    { language: 'xml', pattern: '**/*.tasks' },
    { language: 'msbuild', pattern: '**/*.*' }
];

/**
 * Initialisation options for the MSBuild language server.
 */
export interface LanguageServerInitializationOptions {
    expandGlobalPropertiesFromVSCodeVariables?: boolean;
};

/**
 * Called when the extension is activated.
 * 
 * @param context The extension context.
 */
export async function activate(context: vscode.ExtensionContext): Promise<void> {
    outputChannel = vscode.window.createOutputChannel('MSBuild Project Tools');

    const progressOptions: vscode.ProgressOptions = {
        location: vscode.ProgressLocation.Window
    };
    await vscode.window.withProgress(progressOptions, async progress => {
        progress.report({
            message: 'Initialising MSBuild project tools...'
        });

        const hostRuntimeDiscoveryResult = await dotnet.discoverUserRuntime();

        if (!hostRuntimeDiscoveryResult.success) {
            const failureResult = hostRuntimeDiscoveryResult as { failure: dotnet.RuntimeDiscoveryFailure };
            switch (failureResult.failure) {
                case dotnet.RuntimeDiscoveryFailure.DotnetNotFoundInPath:
                    outputChannel.appendLine('"dotnet" command was not found in the PATH. Please make sure "dotnet" is available from the PATH and reload extension since it is required for it to work');
                    vscode.window.showErrorMessage('"dotnet" was not found in the PATH (see the output window for details).');
                    break;
                case dotnet.RuntimeDiscoveryFailure.ErrorWhileGettingRuntimesList:
                    outputChannel.appendLine('Error occured while trying to execute "dotnet --list-runtimes" command');
                    vscode.window.showErrorMessage('Error occured while trying to invoke "dotnet" command (see the output window for details).');
                    break;
            }
            return;
        }

        await createLanguageClient(context, hostRuntimeDiscoveryResult);

        context.subscriptions.push(
            handleExpressionAutoClose()
        );

        registerInternalCommands(context);
    });

    context.subscriptions.push(
        vscode.workspace.onDidChangeConfiguration(async args => {
            if (languageClient && args.affectsConfiguration('msbuildProjectTools.logging.trace')) {
                const trace = vscode.workspace.getConfiguration('msbuildProjectTools.logging').trace ? Trace.Verbose : Trace.Off;
                await languageClient.setTrace(trace);
            }
        })
    );
}

/**
 * Called when the extension is deactivated.
 */
export async function deactivate(): Promise<void> {
    await languageClient.stop();
}

/**
 * Create the MSBuild language client.
 * 
 * @param context The current extension context.
 * @returns A promise that resolves to the language client.
 */
async function createLanguageClient(context: vscode.ExtensionContext, dotnetOnHost: { dotnetExecutablePath: string, canBeUsedForRunningLanguageServer: boolean }): Promise<void> {
    statusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right, 50);
    context.subscriptions.push(statusBarItem);

    statusBarItem.text = '$(check) MSBuild Project';
    statusBarItem.tooltip = 'MSBuild Project Tools';
    statusBarItem.hide();

    outputChannel.appendLine('Starting MSBuild language service...');

    const languageServerInitializationOptions : LanguageServerInitializationOptions = {
        expandGlobalPropertiesFromVSCodeVariables: true,
    };

    const clientOptions: LanguageClientOptions = {
        synchronize: {
            configurationSection: 'msbuildProjectTools'
        },
        diagnosticCollectionName: 'MSBuild Project',
        errorHandler: {
            error: (error, message, count) => {
                if (count > 2)  // Don't be annoying
                    return { action: ErrorAction.Shutdown };

                console.log(message);
                console.log(error);

                if (message)
                    outputChannel.appendLine(`The MSBuild language server encountered an unexpected error: ${message}\n\n${error}`);
                else
                    outputChannel.appendLine(`The MSBuild language server encountered an unexpected error.\n\n${error}`);

                return { action: ErrorAction.Continue };
            },
            closed: () => { return { action: CloseAction.DoNotRestart } }
        },
        initializationOptions: languageServerInitializationOptions,
        initializationFailedHandler(error: Error) : boolean {
            console.log(error);
            
            outputChannel.appendLine(`Failed to initialise the MSBuild language server.\n\n${error}`);
            vscode.window.showErrorMessage(`Failed to initialise MSBuild language server.\n\n${error}`);

            return false; // Don't attempt to restart the language server.
        },
        revealOutputChannelOn: RevealOutputChannelOn.Never
    };

    const loggingConfig = vscode.workspace.getConfiguration('msbuildProjectTools.logging');

    const seqLoggingSettings = loggingConfig.seq;
    if (seqLoggingSettings?.url) {
        languageServerEnvironment['MSBUILD_PROJECT_TOOLS_SEQ_URL'] = seqLoggingSettings.url;
        languageServerEnvironment['MSBUILD_PROJECT_TOOLS_SEQ_API_KEY'] = seqLoggingSettings.apiKey;
    }

    if (loggingConfig.file) {
        languageServerEnvironment['MSBUILD_PROJECT_TOOLS_LOG_FILE'] = loggingConfig.file;
    }

    if (loggingConfig.level === 'Verbose') {
        languageServerEnvironment['MSBUILD_PROJECT_TOOLS_VERBOSE_LOGGING'] = '1';
    }

    const serverAssembly = context.asAbsolutePath('language-server/MSBuildProjectTools.LanguageServer.Host.dll');
    let dotnetForLanguageServer = dotnetOnHost.dotnetExecutablePath;

    if (!dotnetOnHost.canBeUsedForRunningLanguageServer) {
        const isolatedDotnet = await dotnet.acquireIsolatedRuntime(context.extension.id);

        if (isolatedDotnet === null) {
            const baseErrorMessage = 'Cannot enable the MSBuild language service: unable to acquire isolated .NET runtime';
            outputChannel.appendLine(baseErrorMessage + ". See '.NET Runtime' channel for more info");
            await vscode.window.showErrorMessage(baseErrorMessage);
            return;
        }

        await dotnet.acquireDependencies(isolatedDotnet, serverAssembly);

        dotnetForLanguageServer = isolatedDotnet;
        outputChannel.appendLine("Using isolated .NET runtime");
    } else {
        languageServerEnvironment['DOTNET_ROLL_FORWARD'] = 'LatestMajor';
        languageServerEnvironment['DOTNET_ROLL_FORWARD_TO_PRERELEASE'] = '1';

        outputChannel.appendLine("Using .NET runtime from the host");
    }

    languageServerEnvironment['DOTNET_HOST_PATH'] = realpathSync(dotnetOnHost.dotnetExecutablePath);

    const serverOptions: ServerOptions = {
        command: dotnetForLanguageServer,
        args: [serverAssembly],
        options: {
            env: languageServerEnvironment
        }
    };

    languageClient = new LanguageClient('MSBuild Language Service', serverOptions, clientOptions);
    const trace = loggingConfig.trace ? Trace.Verbose : Trace.Off;
    await languageClient.setTrace(trace);

    try {
        await languageClient.start();
        handleBusyNotifications(languageClient, statusBarItem);
        outputChannel.appendLine('MSBuild language service is running.');
    }
    catch (startFailed) {
        outputChannel.appendLine(`Failed to start MSBuild language service.\n\n${startFailed}`);
        return;
    }
}

/**
 * Handle document-change events to automatically insert a closing parenthesis for common MSBuild expressions.
 */
function handleExpressionAutoClose(): vscode.Disposable {
    return vscode.workspace.onDidChangeTextDocument(async args => {
        if (!vscode.languages.match(projectDocumentSelector, args.document))
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
