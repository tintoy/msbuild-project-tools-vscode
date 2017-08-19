import { child_process } from 'mz';

import * as executables from './executables';

/**
 * Get the current version of .NET Core.
 * 
 * @returns A promise that resolves to the current .NET Core version, or null if the version could not be determined.
 */
export async function getVersion(): Promise<string> {
    const dotnetExecutable = await executables.find('dotnet');
    if (dotnetExecutable === null)
        return '';

    const [stdOut, stdError] = await child_process.execFile(dotnetExecutable, [ '--version' ]);
    
    return stdOut.trim();
}
