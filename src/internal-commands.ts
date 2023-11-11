import * as vscode from 'vscode';

/**
 * Register the extension's internal commands.
 * 
 * @param context The current extension context.
 */
export function registerInternalCommands(context: vscode.ExtensionContext): void
{
    context.subscriptions.push(
        vscode.commands.registerTextEditorCommand('msbuildProjectTools.internal.moveAndSuggest', moveAndSuggest)
    );
}

/**
 * Move the cursor and trigger completion.
 * 
 * @param editor The text editor where the command was invoked.
 * @param edit The text editor's edit facility.
 * @param moveTo The logical direction in which to move (e.g. 'left', 'right', 'up', 'down', etc).
 * @param moveBy The unit to move by (e.g. 'line', 'wrappedLine', 'character', 'halfLine').
 * @param moveCount The number of units to move by.
 */
async function moveAndSuggest(editor: vscode.TextEditor, edit: vscode.TextEditorEdit, moveTo: string, moveBy: string, moveCount: number): Promise<void> {
    if (!moveTo || !moveBy || !moveCount)
        return;

    // Move.
    await vscode.commands.executeCommand('cursorMove', {
        value: moveCount,
        to: moveTo,
        by: moveBy
    });

    // Trigger completion.
    await vscode.commands.executeCommand('editor.action.triggerSuggest');
}
