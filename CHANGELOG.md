# Change Log

## v0.1.2

* Provide intellisense for items whose conditions evaluate to `false`.
* Handle `Import` elements that give rise to multiple imported projects (this already worked correctly for SDK-style imports).
* Initial support for master and sub projects.

## v0.1.1

* Use a patched version of `Microsoft.Language.Xml` that behaves correctly in non-windows environments (issues with CR vs CRLF line-endings).
* Improve tooltips on hover.

## v0.1.0

* Fix handling of non-windows line endings.

## v0.1.0-rc1

* Fix cross-platform path handling.

## v0.1.0-beta2

* More informative tooltips on hover
* Handle multiple items originating from a single item group element in the XML.
* Improved error handling

## v0.1.0-beta1

* Implement go-to-definition for project-style and SDK-style imports.
* Detect .NET Core version on startup, and fall back to classic completion provider if >= 2.0.0 is not available.

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
