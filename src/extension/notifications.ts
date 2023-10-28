import * as vscode from 'vscode';
import { NotificationType } from 'vscode-jsonrpc';
import { LanguageClient } from 'vscode-languageclient/lib/node/main';

/**
 * Represents a custom notification indicating that the MSBuild language service is or is not busy.
 */
export interface BusyNotification {
    /** Is the language service busy? */
    isBusy: boolean;

    /** If the language service is busy, a short message describing why. */
    message: string;
}

/**
 * Well-known notification types.
 */
export namespace NotificationTypes {
    /** The {@link BusyNotification} type. */
    export const busy = new NotificationType<BusyNotification>('msbuild/busy');
}

/**
 * Configure the language client to handle "language service is busy" notifications.
 * 
 * @param languageClient The MSBuild language client.
 */
export function handleBusyNotifications(languageClient: LanguageClient, statusBarItem: vscode.StatusBarItem): void {
    languageClient.onNotification(NotificationTypes.busy, notification => {
        if (notification.isBusy) {
            statusBarItem.text = '$(watch) MSBuild Project';
            statusBarItem.tooltip = 'MSBuild Project Tools: ' + notification.message;
            statusBarItem.show();
        } else {
            statusBarItem.text = '$(check) MSBuild Project';
            statusBarItem.tooltip = 'MSBuild Project Tools';
            statusBarItem.hide();
        }
    });
}
