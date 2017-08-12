# MSBuild project file tools

An extension for VS Code that provides auto-complete when editing `<PackageReference />` elements MSBuild project files.

## Usage

* When you're editing your project file, type `pr` then press `tab` to insert a `PackageReference` element.
* Move to the `Include` or `Version` attribute of your `PackageReference` element and press `ctrl+space` to bring up a list of package Ids / versions.

## Installation

Since this extension is not yet available from the VS marketplace, simply [download](https://github.com/tintoy/msbuild-project-tools-vscode/releases/latest) the VSIX package for the latest release and install it by choosing "Install from VSIX" from the menu on the top right of the extensions panel.

## Notes

This extension uses the NuGet v3 API to resolve package names and versions. The API is pretty slow, unfortunately; I'll try to improve performance / result caching in the next release. For now, it only searches the [nuget.org](https://nuget.org) package feed but in a future release it will respect package sources defined in `NuGet.config`.

The parsing of project XML occurs on a line-by-line basis, so if your `PackageReference` element spans more than one line, auto-complete will not be available.

## Questions / bug reports

If you have questions, feature requests, or would like to report a bug, please feel free to reach out by creating an issue. When reporting a bug, please try to include as much information as possible about what you were doing at the time, what you expected to happen, and what actually happened.

If you're interested in collaborating that'd be great, too :-)
