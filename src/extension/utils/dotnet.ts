import { child_process } from 'mz';
import { EOL } from "os";

import * as executables from './executables';

import * as semver from 'semver';

/**
 * Get the current version of .NET Core.
 * 
 * @returns A promise that resolves to the current host version, or null if the version could not be determined.
 */
export async function getHostVersion(): Promise<semver.SemVer | null> {
    const hostOutput = await runDotNetHost('--version');
    if (!hostOutput.stdOutLines) {
        throw new Error(`Failed to determine .NET host version.${EOL}Host STDOUT: ${hostOutput.stdOutLines.join(EOL)}\nHost STDERR: ${hostOutput.stdErrorLines.join(EOL)}`);
    }

    const hostVersion = semver.parse(hostOutput.stdOutLines[0]);

    return hostVersion;
}

/**
 * Get the currently-supported runtimes and versions of .NET / .NET Core.
 * 
 * @returns A promise that resolves to a `Map<string, SemVer[]>` from runtime name to supported versions.
 */
export async function getRuntimeVersions(): Promise<Map<string, semver.SemVer[]>> {
    const hostOutput = await runDotNetHost('--list-runtimes');
    if (!hostOutput.stdOutLines) {
        throw new Error(`Failed to determine available .NET runtime versions.${EOL}Host STDOUT: ${hostOutput.stdOutLines.join(EOL)}\nHost STDERR: ${hostOutput.stdErrorLines.join(EOL)}`);
    }

    const runtimes: Map<string, semver.SemVer[]> = new Map<string, semver.SemVer[]>();

    for (let outputIndex = 0; outputIndex < hostOutput.stdOutLines.length; outputIndex++) {
        const runtimeInfoComponents: string[] = hostOutput.stdOutLines[outputIndex].split(' ', 3);
        if (runtimeInfoComponents.length < 3)
            continue;

        const runtimeName: string = runtimeInfoComponents[0];
        const runtimeVersion = semver.parse(runtimeInfoComponents[1]);
        if (!runtimeVersion)
            continue;

        let versions: semver.SemVer[] = runtimes.get(runtimeName);
        if (!versions) {
            versions = [];
            runtimes.set(runtimeName, versions);
        }

        versions.push(runtimeVersion);
    }

    return runtimes;
}

/**
 * The output (STDOUT and STDERR) from an invocation of the "dotnet" command-line tool.
 */
interface DotNetHostOutput {
    stdOutLines: string[];
    stdErrorLines: string[];
}

/**
 * Invoke the "dotnet" command-line host.
 * 
 * @param args The host arguments.
 * @returns The host output.
 */
async function runDotNetHost(args: string | string[]): Promise<DotNetHostOutput> {
    const dotnetExecutable = await executables.find('dotnet');
    if (dotnetExecutable === null) {
        return {
            stdOutLines: [],
            stdErrorLines: [
                "Failed to locate 'dotnet' executable."
            ],
        };
    }

    if (!Array.isArray(args))
        args = [args];

    const [stdOut, stdError] = await child_process.execFile(dotnetExecutable, args);

    console.log('runDotNetHost -> [stdOut, stdError]', [stdOut, stdError]);

    const stdOutLines: string[] = stdOut.trim().split(EOL);
    const stdErrorLines: string[] = stdError.trim().split(EOL);

    return {
        stdOutLines,
        stdErrorLines
    }
}
