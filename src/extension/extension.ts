'use strict';

import { default as axios } from 'axios';
import * as path from 'path';
import * as vscode from 'vscode';
import { LanguageClient, ServerOptions, LanguageClientOptions, ErrorAction, CloseAction } from 'vscode-languageclient';

import * as executables from './utils/executables';
import { PackageReferenceCompletionProvider } from './providers/package-reference-completion';
import { Trace } from 'vscode-jsonrpc/lib/main';
import { Message } from 'vscode-jsonrpc/lib/messages';

/**
 * Called when the extension is activated.
 * 
 * @param context The extension context.
 */
export async function activate(context: vscode.ExtensionContext): Promise<void> {
    const languageClient = await createLanguageClient(context);
    
    const outputChannel = languageClient.outputChannel;
    outputChannel.appendLine('Starting MSBuild language service...');
    context.subscriptions.push(
        languageClient.start()
    );
    await languageClient.onReady();
    outputChannel.appendLine('MSBuild language service is running.');
}

/**
 * Called when the extension is deactivated.
 */
export function deactivate(): void {
    // Nothing to clean up.
}

/**
 * Create the MSBuild language client.
 * 
 * @param context The current extension context.
 * @returns A promise that resolves to the language client.
 */
async function createLanguageClient(context: vscode.ExtensionContext): Promise<LanguageClient> {
    const clientOptions: LanguageClientOptions = {
        documentSelector: [{
            language: 'xml',
            pattern: '*.csproj'
        }],
        synchronize: {
            // Synchronize the setting section 'languageServerExample' to the server
            configurationSection: 'msbuildProjectFileTools',
            // Notify the server about file changes to '.clientrc files contain in the workspace
            fileEvents: vscode.workspace.createFileSystemWatcher('**/.clientrc')
        }
    };

    const dotNetExecutable = await executables.find('dotnet');
    const serverAssembly = context.asAbsolutePath('src/LanguageServer/bin/Debug/netcoreapp2.0/publish/LanguageServer.dll');
    const serverOptions: ServerOptions = {
        command: dotNetExecutable,
        args: [ serverAssembly ],
    };

    return new LanguageClient('MSBuild Project File Tools', serverOptions, clientOptions);
}
