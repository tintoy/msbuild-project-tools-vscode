'use strict';

const assert = require('node:assert/strict');
const path = require('node:path');
const test = require('node:test');
const { expandVSCodeVariables } = require('../out/variable-expansion');

const context = {
    environment: {
        BUILD_CONFIGURATION: 'Release'
    },
    userHome: 'C:\\Users\\test',
    workspaceFolders: [
        { name: 'Main', path: 'C:\\src\\main' },
        { name: 'Shared', path: 'C:\\src\\shared' }
    ]
};

test('expands workspace variables and legacy aliases', () => {
    assert.equal(expandVSCodeVariables('${workspaceFolder}/Foo.sln', context), 'C:\\src\\main/Foo.sln');
    assert.equal(expandVSCodeVariables('${workspaceRoot}',           context), 'C:\\src\\main');
    assert.equal(expandVSCodeVariables('${workspaceFolderBasename}', context), 'Main');
    assert.equal(expandVSCodeVariables('${workspaceRootFolderName}', context), 'Main');
});

test('expands named workspace, environment, and host variables', () => {
    assert.equal(expandVSCodeVariables('${workspaceFolder:Shared}',  context), 'C:\\src\\shared');
    assert.equal(expandVSCodeVariables('${env:BUILD_CONFIGURATION}', context), 'Release');
    assert.equal(expandVSCodeVariables('${userHome}',                context), 'C:\\Users\\test');
    assert.equal(expandVSCodeVariables('${pathSeparator}',           context), path.sep);
});

test('recursively expands variables while safely stopping cycles', () => {
    const recursiveContext = {
        ...context,
        environment: {
            ROOT: '${workspaceFolder}',
            FIRST: '${env:SECOND}',
            SECOND: '${env:FIRST}'
        }
    };

    assert.equal(expandVSCodeVariables('${env:ROOT}/Foo.sln', recursiveContext), 'C:\\src\\main/Foo.sln');

    // There is no right answer for this test between FIRST/SECOND, recursion depth is arbitrarily set to 10 atow.
    const result = expandVSCodeVariables('${env:FIRST}', recursiveContext);
    assert([ '${env:FIRST}', '${env:SECOND}' ].includes(result));
});

test('leaves unsupported and unavailable variables unchanged', () => {
    assert.equal(expandVSCodeVariables('${command:chooseFolder}',    context), '${command:chooseFolder}');
    assert.equal(expandVSCodeVariables('${workspaceFolder:Missing}', context), '${workspaceFolder:Missing}');
    assert.equal(expandVSCodeVariables('${env:MISSING}',             context), '${env:MISSING}');
});