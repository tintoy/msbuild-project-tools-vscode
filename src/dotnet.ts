import { exec } from 'child_process';
import * as vscode from 'vscode';
import semver = require('semver');
import which = require('which');

const requiredDotnetRuntimeVersion = '8.0';

export interface RuntimeDiscoveryResult {
    dotnetExecutablePath : string;
    canBeUsedForRunningLanguageServer : boolean;
}

export async function discoverUserRuntime() : Promise<RuntimeDiscoveryResult | null> {
    const dotnetHostPath = await which('dotnet', { nothrow: true });

    if (dotnetHostPath === null) {
        return null;
    }

    const dotnetHostVersionOutput = await new Promise<string>((resolve, reject) => {
        exec(`"${dotnetHostPath}" --version`, (error, stdOut, stdErr) => {
            if (error) {
                reject(error);
            } else if (stdErr) {
                reject(new Error(stdErr));
            } else {
                resolve(stdOut);
            }
        });
    });

    const dotnetHostVersion = dotnetHostVersionOutput.trim();

    return {
        dotnetExecutablePath: dotnetHostPath,
        canBeUsedForRunningLanguageServer: semver.gte(dotnetHostVersion, `${requiredDotnetRuntimeVersion}.0`)
    };
}

interface DotnetAcquireResult {
    dotnetPath: string;
}

export async function acquireIsolatedRuntime(extensionId: string) : Promise<string | null> {
    const dotnetAcquireArgs = { version: requiredDotnetRuntimeVersion, requestingExtensionId: extensionId };
    let status = await vscode.commands.executeCommand<DotnetAcquireResult>('dotnet.acquireStatus', dotnetAcquireArgs);
    if (status === undefined) {
        await vscode.commands.executeCommand('dotnet.showAcquisitionLog');
        status = await vscode.commands.executeCommand<DotnetAcquireResult>('dotnet.acquire', dotnetAcquireArgs);
    }

    if (!status?.dotnetPath) {
        return null;
    }

    return status.dotnetPath;
}

export async function acquireDependencies(dotnetExecutablePath : string, dotnetAppPath: string) : Promise<void> {
    await vscode.commands.executeCommand('dotnet.ensureDotnetDependencies', { command: dotnetExecutablePath, arguments: [dotnetAppPath] });
}
