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

1. `npm run build-language-server`
2. `vsce package`

# CI pipelines

* [development (`master` branch)](https://dev.azure.com/tintoy-dev/msbuild-project-tools-vscode/_build?definitionId=3)
* [release (`release/vX.Y.Z` and `preview/vX.Y.Z` branches)](https://dev.azure.com/tintoy-dev/msbuild-project-tools-vscode/_build?definitionId=5)

## Publishing to VS Gallery

To publish the extension to the VS Gallery, push your commits to a branch matching `release/vX.Y.Z` or `preview/vX.Y.Z` (where `X.Y.Z` is a SemVer-compliant version number) - `v0.7.1`, for example.

If the final SemVersion includes a pre-release tag, it will be published as a pre-release version in the gallery.

**Note**: We follow the convention that even-numbered minor versions are stable releases, and odd-numbered minor versions are previews (due to limitations in the VS Gallery's publishing model).
