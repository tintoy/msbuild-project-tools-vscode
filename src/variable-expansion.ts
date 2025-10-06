import * as vscode from 'vscode';
import { CancellationToken, HandlerResult, RequestHandler, RequestMessage, RequestType, RequestType0, RequestType1, ResponseError, ResponseMessage } from 'vscode-jsonrpc';
import { LanguageClient } from 'vscode-languageclient/lib/node/main';

// Ugly, but their type-defs are broken.
type ExpandVSCodeVariables = (text: string, recursive?: boolean) => string;
const expandVSCodeVariables: ExpandVSCodeVariables = require('vscode-variables');

/**
 * Represents a custom Request for the LSP client to expand the specified variables.
 */
export interface ExpandVariablesRequest {
    variables: Variables;
}

/**
 * Represents a custom Response from the LSP client, expanding the specified variables.
 */
export interface ExpandVariablesResponse {
    variables: Variables;
}

/**
 * Well-known Request types.
 */
export namespace RequestTypes {
    /** The {@link ExpandVariablesRequest} type. */
    export const expandVariables = new RequestType<ExpandVariablesRequest, ExpandVariablesResponse, ResponseError>('variables/expand');
}

export interface Variables {
    [name: string] : string;
}

export interface VariableExpander {
    expand(variables: Variables): Promise<Variables>;
}

export class VSCodeVariableExpander implements VariableExpander {
    constructor() {
    }

    expand(variables: Variables): Promise<Variables> {
        const expandedVariables : Variables = {};

        for (let variableName in Object.getOwnPropertyNames(variables))
            expandedVariables[variableName] = expandVSCodeVariables(variables[variableName]);
        
        return Promise.resolve(expandedVariables);
    }
}

/**
 * Configure the language client to handle variable-expansion requests.
 * 
 * @param languageClient The MSBuild language client.
 */
export function handleVariableExpansionRequests(languageClient: LanguageClient, variableExpander?: VariableExpander): void {
    variableExpander ??= new VSCodeVariableExpander();
    
    languageClient.onRequest("variables/expand", async (request: ExpandVariablesRequest): Promise<ExpandVariablesResponse> => {
        const variables: Variables = request.variables;
        const expandedVariables = await variableExpander.expand(variables);
        
        return {
            variables: expandedVariables,
        };
    });
}
