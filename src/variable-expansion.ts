import * as path from 'path';

/**
 * Values used when expanding the subset of VS Code variables supported in
 * MSBuild global-property overrides.
 */
export interface VariableExpansionContext {
    environment: NodeJS.ProcessEnv;
    userHome: string | undefined;
    workspaceFolders: {
        name: string;
        path: string;
    }[];
}

/**
 * Expand supported VS Code variables in a string.
 *
 * Unsupported variables, and variables whose values cannot be determined, are
 * left unchanged so they can be diagnosed or interpreted by another consumer.
 */
export function expandVSCodeVariables(value: string, context: VariableExpansionContext): string {
    let expandedValue = value;
    for (let i = 0; i < 10; i += 1) {
        // Search for all elements like: "${...}", 'variableText' is the entire contents of '...'
        const nextValue = expandedValue.replaceAll(
            /\$\{([^}]+)\}/g,
            (variableToken: string, variableText: string) => evaluateVariable(variableToken, variableText, context));
        if (nextValue === expandedValue) {
            break;
        }
        expandedValue = nextValue;
    }

    return expandedValue;
}

function evaluateVariable(variableToken: string, variableText: string, context: VariableExpansionContext): string {
    const [ variableName, variableParam ] = variableText.split(':', 2);
    const hasParam = !!variableParam;

    // variableToken:   ${env:HOME}
    // variableText:      env:HOME
    // variableName:      env
    // variableParam:         HOME

    if (variableName === 'userHome' && !hasParam) {
        return context.userHome || variableToken;
    }
    if (variableName === 'pathSeparator' && !hasParam) {
        return path.sep;
    }
    if (variableName === 'env' && hasParam) {
        const environmentVariable = context.environment[variableParam];
        return environmentVariable === undefined ? variableToken : environmentVariable;
    }

    const getWorkspaceFolder = function(folderName: string | undefined) {
        return folderName
            ? context.workspaceFolders.find(folder => folder.name === folderName)
            : context.workspaceFolders[0];
    }
    if (variableName === 'workspaceFolder' || variableName === 'workspaceRoot') {
        return getWorkspaceFolder(variableParam)?.path || variableToken;
    }
    if (variableName === 'workspaceFolderBasename' || variableName === 'workspaceRootFolderName') {
        return getWorkspaceFolder(variableParam)?.name || variableToken;
    }

    return variableToken;
}
