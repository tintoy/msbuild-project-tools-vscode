# Building MSBuild Project Tools for VS Code

You'll need:

1. .NET 6.0.0 or newer
2. NodeJS
3. VSCE  
   `npm install -g @vscode/vsce`
4. Powershell (already there by default on Windows)

Don't forget to update LSP submodule after pulling the repo:

1. `git submodule init`
2. `git submodule update`

To build:

1. `npm install`
2. `powershell Publish-LanguageServer.ps1 --dev`

To debug:

1. Open VS Code, and hit F5. Both LSP and extension client will be built automatically. A new instance of VS Code will be opened and a debug session will be activated

To create a VSIX package:

1. `vsce package`
