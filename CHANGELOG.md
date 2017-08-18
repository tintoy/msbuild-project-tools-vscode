# Change Log

## v0.1.0-alpha2

* Add configuration property (`msbuildProjectFileTools.languageService.enable`) to switch between MSBuild language engine and classic completion provider.

## v0.1.0-alpha1

* The extension now uses a language server based on Microsoft's Language Server Protocol.
* Tooltips for MSBuild objects in the project file.
* Support for any configured (remote) package source.  
  We're using the new NuGet client libraries so it should understand global, solution-local, and project-local package sources.
* Highly-improved handling of project files with broken or invalid XML (thanks to `Microsoft.Language.Xml`).

## v0.0.2

* Resolve the URL for the NuGet v3 AutoComplete API at extension startup.

## v0.0.1

* Initial release.
