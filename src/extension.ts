'use strict';

import * as vscode from 'vscode';

import { PackageReferenceCompletionProvider } from './providers/package-reference-completion';

/**
 * Called when the extension is activated.
 * 
 * @param context The extension context.
 */
export async function activate(context: vscode.ExtensionContext): Promise<void> {
    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(
            { language: 'xml', pattern: '**/*.*proj' }, 
            new PackageReferenceCompletionProvider()
        )
    );
}

/**
 * Called when the extension is deactivated.
 */
export function deactivate(): void {
    // Nothing to clean up.
}
