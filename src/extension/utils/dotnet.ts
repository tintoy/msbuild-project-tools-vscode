import { child_process } from 'mz';
import { EOL } from "os";

import * as executables from './executables';

/**
 * Get the current version of .NET Core.
 * 
 * @returns A promise that resolves to the current host version, or null if the version could not be determined.
 */
export async function getHostVersion(): Promise<string> {
    const dotnetExecutable = await executables.find('dotnet');
    if (dotnetExecutable === null)
        return '';

    const [stdOut, stdError] = await child_process.execFile(dotnetExecutable, [ '--info' ]);

    const lines: string[] = stdOut.trim().split(EOL);
    if (lines.length <= 1)
        return stdError.trim();

    // Assumes new versions of "dotnet --info" will continue to return the same set of sections in the same order (even if their names may be localised).
    let infoSectionName: string = null;
    let runtimeEnvironmentSectionName: string = null;
    let globalJsonSectionName: string = null;
    let hostSectionName: string = null;
    let sdkListSectionName: string = null;
    let runtimeListSectionName: string = null;

    let sectionName: string = "unknown";
    
    let sectionData = new Map<string, string[]>();
    let currentSection: string[] = [];
    sectionData[sectionName] = currentSection;

    for (let lineIndex = 0; lineIndex < lines.length; lineIndex++) {
        const currentLine = lines[lineIndex];
        if (currentLine.trim().length === 0)
            continue;

        if (currentLine.startsWith(' ')) {
            // Section data
            currentSection.push(
                currentLine.trim()
            );
        } else {
            // New section
            sectionName = currentLine.trim();
            currentSection = sectionData.get(sectionName);
            if (!currentSection) {
                currentSection = [];
                sectionData.set(sectionName, currentSection);
            }

            if (!infoSectionName)
                infoSectionName = sectionName;
            else if (!runtimeEnvironmentSectionName)
                runtimeEnvironmentSectionName = sectionName;
            else if (!globalJsonSectionName)
                globalJsonSectionName = sectionName;
            else if (!hostSectionName)
                hostSectionName = sectionName;
            else if (!sdkListSectionName)
                sdkListSectionName = sectionName;
            else if (!runtimeListSectionName)
                runtimeListSectionName = sectionName;
        }
    }

    if (!hostSectionName)
        throw new Error('Failed to find the Host section in output from "dotnet --info".');

    const hostSection: string[] = sectionData.get(hostSectionName);
    const hostVersionInfo = hostSection.find(
        keyValuePair => keyValuePair.startsWith('Version: ')
    );
    if (!hostVersionInfo)
        throw new Error('Failed to find the Host version information in output from "dotnet --info".');
    
    const hostVersion: string = hostVersionInfo.split(':')[1].trim();
    
    return hostVersion;
}
