# MSBuild project file tools

An extension for VS Code that provides intellisense for MSBuild project files, including auto-complete for `<PackageReference>` elements.

![PackageReference completion](docs/images/extension-in-action.gif)

**Note**: there are new features in the extension that use an out-of-process language server. This is disabled by default but you can enable it by setting `msbuildProjectFileTools.languageService.enable` to `true` in your VSCode preferences. You don't have to use it, but it does provide a lot of additional functionality.

## Usage

* When you're editing your project file, type `pr` then press `tab` to insert a `PackageReference` element.
* Move to the `Include` or `Version` attribute of your `PackageReference` element and press `ctrl+space` to bring up a list of package Ids / versions.
* Hover the mouse over targets, items, and properties to see information about them.
* Go-to-definition is implemented for both SDK-style and regular project imports.
* Document symbols are supported for imports, targets, items, and properties.

## Installation

You can install this extension from the [VS marketplace](https://marketplace.visualstudio.com/items?itemName=tintoy.msbuild-project-tools), or simply [download](https://github.com/tintoy/msbuild-project-tools-vscode/releases/latest) the VSIX package for the latest release and install it by choosing "Install from VSIX" from the menu on the top right of the extensions panel.

## Limitations

* The new language server hasn't been tested extensively on Linux / MacOS (although I've verified that it works for simple cases).
* This extension uses the NuGet v3 API to resolve package names and versions. The API is pretty slow, unfortunately; I'll try to improve performance / result caching in the next release.
* Intellisense is not currently available for items with conditions evaluating to `false` as they are not present in the MSBuild project at runtime. I've fixed this for properties, so there's definitely a way forward; hopefully in the next release.

## Questions / bug reports

If you have questions, feature requests, or would like to report a bug, please feel free to reach out by creating an issue. When reporting a bug, please try to include as much information as possible about what you were doing at the time, what you expected to happen, and what actually happened.

If you're interested in collaborating that'd be great, too :-)
