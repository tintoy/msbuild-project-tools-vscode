'use strict';

import { default as axios } from 'axios';
import * as vscode from 'vscode';

import { PackageReferenceCompletionProvider } from './providers/package-reference-completion';

/**
 * Called when the extension is activated.
 * 
 * @param context The extension context.
 */
export async function activate(context: vscode.ExtensionContext): Promise<void> {
    const nugetEndPointURLs = await getNuGetV3AutoCompleteEndPoints();

    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(
            { language: 'xml', pattern: '**/*.*proj' }, 
            new PackageReferenceCompletionProvider(
                nugetEndPointURLs[0] // For now, just default to using the primary.
            )
        )
    );
}

/**
 * Called when the extension is deactivated.
 */
export function deactivate(): void {
    // Nothing to clean up.
}

/**
 * Get the current end-points URLs for the NuGet v3 AutoComplete API.
 */
async function getNuGetV3AutoCompleteEndPoints(): Promise<string[]> {
    const nugetIndexResponse = await axios.get('https://api.nuget.org/v3/index.json');
    
    const index: NuGetIndex = nugetIndexResponse.data;
    const autoCompleteEndPoints = index.resources
        .filter(
            resource => resource['@type'] === 'SearchAutocompleteService'
        )
        .map(
            resource => resource['@id']
        );

    return autoCompleteEndPoints;
}

/**
 * Represents the index response from the NuGet v3 API.
 */
export interface NuGetIndex {
    /**
     * Available API resources.
     */
    resources: NuGetApiResource[];
}

/**
 * Represents a NuGet API resource.
 */
export interface NuGetApiResource {
    /**
     * The resource Id (end-point URL).
     */
    '@id': string;

    /**
     * The resource type.
     */
    '@type': string;

    /**
     * An optional comment describing the resource.
     */
    comment?: string;
}
