import * as vscode from 'vscode';
import { NotificationType } from 'vscode-jsonrpc';
import { LanguageClient } from 'vscode-languageclient/lib/main';

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
    export const busy = new NotificationType<BusyNotification, void>('msbuild/busy');
}

/**
 * Configure the language client to handle "language service is busy" notifications.
 * 
 * @param languageClient The MSBuild language client.
 */
export function handleBusyNotifications(languageClient: LanguageClient): void {
    languageClient.onReady().then(() => {

        // AF: A little ugly due to interaction with VSCode's withProgress API, but it works.

        let onBusyDone: () => void;
        let busyPromise: Thenable<void>;
        let busyProgress: vscode.Progress<{message: string}>;

        languageClient.onNotification(NotificationTypes.busy, notification => {
            if (notification.isBusy) {
                if (!busyPromise) {
                    const progressOptions: vscode.ProgressOptions = { location: vscode.ProgressLocation.Window };
                    busyPromise = vscode.window.withProgress(progressOptions, progress => new Promise((accept, reject) => {
                        onBusyDone = accept; // Cache it so we can complete this promise later.
                        busyProgress = progress;
                    }));
                } else if (!busyProgress) {
                    // Race.
                    console.log(notification.message);

                    return; 
                }
                
                busyProgress.report({
                    message: notification.message
                });
            } else if (onBusyDone) {
                if (busyProgress && notification.message) {
                    busyProgress.report({
                        message: notification.message
                    });
                }

                onBusyDone();

                onBusyDone = null;
                busyPromise = null;
                busyProgress = null;
            }
        });
    });
}
