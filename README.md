# MSBuild project file tools

An extension for VS Code that provides intellisense for MSBuild project files, including auto-complete for `<PackageReference>` elements.

![The extension in action](docs/images/extension-in-action.gif)

**Note**: there are features in the extension (marked "language service feature" below) that use an out-of-process language service; this is enabled by default but if all you want is `PackageReference` completion, you can disable the language service by setting `msbuildProjectFileTools.languageService.enable` to `false` in your VSCode preferences. You will need to reload VSCode after changing this setting.

You need .NET Core 2.0.0 or newer installed to use the language service (but your projects can target any version you have installed).

## Usage

* When you're editing your project file, type `pr` then press `tab` to insert a `PackageReference` element.
* Move to the `Include` or `Version` attribute of your `PackageReference` element and press `ctrl+space` to bring up a list of package Ids / versions.
* _Language service feature:_ Hover the mouse over imports, targets, items, properties, and conditions to see information about them.
* _Language service feature:_ Document symbols are supported for imports, targets, items, and properties.
* _Language service feature:_ Go-to-definition is implemented for both SDK-style and regular project imports.
* Basic syntax highlighting of MSBuild expressions in attribute values.  
  To see this highlighting, change the editor language from `XML` to `MSBuild`.

## Installation

You can install this extension from the [VS marketplace](https://marketplace.visualstudio.com/items?itemName=tintoy.msbuild-project-tools), or simply [download](https://github.com/tintoy/msbuild-project-tools-vscode/releases/latest) the VSIX package for the latest release and install it by choosing "Install from VSIX" from the menu on the top right of the extensions panel.

## Limitations

* The new language service hasn't been tested extensively on Linux / MacOS (although I've verified that it works for common use-cases).
* This extension uses the NuGet v3 API to resolve package names and versions. The API is pretty slow, unfortunately; I'll try to improve performance / result caching in a future release.

**Note**: if you open more than one project at a time (or navigate to imported projects), subsequent projects will be loaded into the same MSBuild project collection as the first project. Once you have closed the last project file, the next project file you open will become the master project. The master project will become selectable in a later release.

## Questions / bug reports

If you have questions, feedback, feature requests, or would like to report a bug, please feel free to reach out by creating an issue. When reporting a bug, please try to include as much information as possible about what you were doing at the time, what you expected to happen, and what actually happened.

If you're interested in collaborating that'd be great, too :-)
