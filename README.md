# .NET PackageReference completion for .NET projects

A quick-and-dirty extension for VS Code that provides auto-complete when editing `<PackageReference />` elements MSBuild project files.

_This is a work-in-progress._

## Usage

When you're editing your project file, type `pr` then press `tab` to insert a `PackageReference` element. Move to the `Include` or `Version` attribute of your `PackageReference` element and press `ctrl+space` to bring up a list of package Ids / versions.

## Installation

Since this extension is not available from the VS gallery yet, simply [download](https://github.com/tintoy/dotnet-package-reference-completion/releases/latest) the VSIX package for the latest release and install it by choosing "Install from VSIX" from the menu on the top right of the extensions panel.

## Notes

This extension uses [nuget-client](https://www.npmjs.com/package/nuget-client) to call the NuGet API. The API is pretty slow, unfortunately; I'll try to improve performance / result caching in the next release.

It will respect any `NuGet.config` it finds in the root of the workspace, but does not currently honour the user's global (`$HOME/.nuget/NuGet`) `NuGet.config`.

## Questions / bug reports

If you have questions, feature requests, or would like to report a bug, please feel free to reach out by creating an issue. When reporting a bug, please try to include as much information as possible about what you were doing at the time, what you expected to happen, and what actually happened.

If you're interested in collaborating that'd be great, too :-)
