import { exec } from 'child_process';
import * as vscode from 'vscode';
import semver = require('semver');
import which = require('which');

const requiredDotnetRuntimeVersion = '8.0';

export type RuntimeDiscoveryResult =
    | { success: true, dotnetExecutablePath: string, canBeUsedForRunningLanguageServer: boolean }
    | { success: false, failure: RuntimeDiscoveryFailure };

export const enum RuntimeDiscoveryFailure { DotnetNotFoundInPath, ErrorWhileGettingRuntimesList };

export async function discoverUserRuntime(canUseNewerRuntime: boolean) : Promise<RuntimeDiscoveryResult> {
    const dotnetHostPath = await which('dotnet', { nothrow: true });

    if (dotnetHostPath === null) {
        return { success: false, failure: RuntimeDiscoveryFailure.DotnetNotFoundInPath };
    }

    const dotnetRuntimeListOutput = await new Promise<string | null>((resolve) => {
        exec(`"${dotnetHostPath}" --list-runtimes`, (error, stdOut, stdErr) => {
            return (error || stdErr) ? resolve(null) : resolve(stdOut);
        });
    });

    if (dotnetRuntimeListOutput === null) {
        return { success: false, failure: RuntimeDiscoveryFailure.ErrorWhileGettingRuntimesList };
    }

    const runtimeStringLines = dotnetRuntimeListOutput.match(/[^\r\n]+/g);
    const runtimes = runtimeStringLines.map(line => {
        const match = line.match(/^(.+?)\s+(\d+\.\d+\.\d+)\s+\[(.+?)\]$/);
        if (match) {
            return {
                type: match[1],
                version: match[2],
                path: match[3]
           };
        }
    }).filter(Boolean);

    // Default roll forward policy of .NET is 'minor', so in order to find compatible runtime we need to make sure
    // that we have at least one generic runtime with the same major version as we need
    const requiredMajorVersion = semver.major(`${requiredDotnetRuntimeVersion}.0`);
    const hasCompatibleRuntime = runtimes.filter(r => r.type === "Microsoft.NETCore.App" && (canUseNewerRuntime ? semver.major(r.version) >= requiredMajorVersion : semver.major(r.version) === requiredMajorVersion)).length > 0;

    return {
        success: true,
        dotnetExecutablePath: dotnetHostPath,
        canBeUsedForRunningLanguageServer: hasCompatibleRuntime
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
