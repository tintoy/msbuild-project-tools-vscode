import * as vscode from 'vscode';

const requiredDotnetRuntimeVersion = '6.0';

interface DotnetAcquireResult {
    dotnetPath: string;
}

export async function acquireRuntime(extensionId: string, progress: vscode.Progress<{ message?: string; increment?: number }>) : Promise<string | null> {
    const dotnetAcquireArgs = { version: requiredDotnetRuntimeVersion, requestingExtensionId: extensionId };
    let status = await vscode.commands.executeCommand<DotnetAcquireResult>('dotnet.acquireStatus', dotnetAcquireArgs);
    if (status === undefined) {
        await vscode.commands.executeCommand('dotnet.showAcquisitionLog');

        progress.report({ message: 'Downloading .NET runtime...' });
        status = await vscode.commands.executeCommand<DotnetAcquireResult>('dotnet.acquire', dotnetAcquireArgs);
    }

    if (!status?.dotnetPath) {
        return null;
    }

    return status.dotnetPath;
}

export async function acquireDependencies(dotnetExecutablePath : string, dotnetAppPath: string) : Promise<void> {
    const args = [dotnetAppPath];
    await vscode.commands.executeCommand('dotnet.ensureDotnetDependencies', { command: dotnetExecutablePath, arguments: args });
}
