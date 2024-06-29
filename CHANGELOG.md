# Change Log

# Upcoming

* Extension now more aggressively forces local user runtime and picks up the latest runtime version available (including previews). This makes it possible to use the extension with preview versions of .NET/MSBuild

# v0.6.4

* Update intellisense help content for Content items and CopyToXXXDirectory global item metadata (tintoy/msbuild-project-tools-vscode#148).

# v0.6.3

* Improve handling of concurrent loads for MSBuild sub-projects (fixes tintoy/msbuild-project-tools-server#100).

# v0.6.0

* Extension startup time has been improved.
* The MSBuild language server now runs on .NET 8.
* Isolated runtime for the language server is back, so users no longer need to have a specific version of .NET to be installed to be able to use the extension.

# v0.5.3

* Improve file-access behaviour for project.assets.json (tintoy/msbuild-project-tools-server#82).

# v0.5.2

* Revert usage of the `vscode-dotnet-runtime` extension (tintoy/msbuild-project-tools-vscode#89 - isolated runtimes are problematic if you want to use global SDKs).

# v0.5.1

* Temporary roll-back from v0.5.0 to v0.4.5 (tintoy/msbuild-project-tools-vscode#137). 

# v0.5.0

* The .NET runtime, required to run the language server, is now acquired using the `vscode-dotnet-runtime` extension as a dependency (so you don't need to have that specific version of .NET on your machine installed to use the extension)
* All completion are now shown by default
* Basic integration with `redhat.vscode-xml` extension is now provided, so you can use its features for `msbuild` language
* Language server, packed with the extension, is now built in release mode
* Extension bundle size has been significantly optimized
* Dependencies have been updaded to address known security vulnerabilities

# v0.4.9

* Wait for redirected STDOUT/STDERR streams to complete when launching dotnet host process (tintoy/msbuild-project-tools-vscode#105, tintoy/msbuild-project-tools-server#28).  
  Thanks, @tillig!

# v0.4.8

* Selectively enable COREHOST_TRACE when launching dotnet executable to probe .NET SDKs (tintoy/msbuild-project-tools-vscode#105, tintoy/msbuild-project-tools-server#28).

# v0.4.7

* Enable logging from .NET / MSBuild-engine discovery logic during language-server startup (tintoy/msbuild-project-tools-server#28).

# v0.4.6

* Simplify logic for detecting .NET host version (tintoy/msbuild-project-tools-vscode#99).

# v0.4.5

* Improve parsing of output from `dotnet --info` (tintoy/msbuild-project-tools-vscode#98).

# v0.4.4

* Always roll forward to the latest (stable) installed version of the runtime (tintoy/msbuild-project-tools-vscode#90).
* Mark extension as a workspace extension to enable correct behaviour in remote scenarios (tintoy/msbuild-project-tools-vscode#99).

# 0.4.3

* Fix ArgumentNullException from NuGet client library when requesting package version completions (tintoy/msbuild-project-tools-vscode#91).

# 0.4.0

* Support for manually ignoring configured package sources using the `msbuildProjectTools.nuget.ignorePackageSources` extension setting (tintoy/msbuild-project-tools-server#24).
* Support for automatically ignoring configured package sources when the v3 service index indicates that they don't support the NuGet completion API (tintoy/msbuild-project-tools-server#24).

## 0.3.16

* Add support for additional wel-known metadata of ProjectReference items (tintoy/msbuild-project-tools-server#26).

## 0.3.15

* Improve error handling when project assets file was not found while updating NuGet package references for a project (tintoy/msbuild-project-tools-server#24).

## 0.3.14

* Correctly handle preview versions of the .NET SDK when discovering MSBuild instances (tintoy/msbuild-project-tools-vscode#74).

## 0.3.13

* Improve detection logic for .NET host version during extension startup (tintoy/msbuild-project-tools-vscode#73).

## 0.3.12

* Remove legacy ("classic") completion provider.
* Fix MSBuild-dependent tests that break in CI when the 5.0 SDK is also installed (tintoy/msbuild-project-tools-server#20).
* Upgrade language server to target .NET 5.0 (tintoy/msbuild-project-tools-server#22).

## 0.3.11

* Upgrade the language service to use the .NET Core 3.1 runtime (tintoy/msbuild-project-tools-server#20).

## 0.3.10

* Always use the MSBuild engine from the newest version of the .NET Core SDK (tintoy/msbuild-project-tools-server#19).

## v0.3.8

* Completions now correctly replace trigger characters, if any (tintoy/msbuild-project-tools-vscode#67).

## v0.3.7

* Explicitly watch parent process for termination (tintoy/msbuild-project-tools-vscode#53).

## v0.3.6

* Update MSBuild engine packages to v16.5.0 (tintoy/msbuild-project-tools-vscode#66).

## v0.3.4

* Add IntelliSense for `GenerateDocumentationFile` property (tintoy/msbuild-project-tools-vscode#60).

## v0.3.3

* Use v16.4.0 of the MSBuild engine (tintoy/msbuild-project-tools-vscode#59).

## v0.3.2

* Improved error reporting when language service cannot be started (tintoy/msbuild-project-tools-server#17).

## v0.3.1

* Language service now targets .NET Core 3.0 (tintoy/msbuild-project-tools-server#17).

## v0.2.55

* Improve calculation logic for MSBuild ToolsVersion (tintoy/msbuild-project-tools-server#16).

## v0.2.54

* Use tab-character in completion text (tintoy/msbuild-project-tools-server#13).

## v0.2.53

* Upgrade MSBuild packages to v15.9.20 (tintoy/msbuild-project-tools-server#14).

## v0.2.52

* Add UserSecretsId to well-known properties (tintoy/msbuild-project-tools-vscode#48).

## v0.2.51

* Use correct MSBuild SDK folder for .NET Core 3.0 and newer (tintoy/msbuild-project-tools-vscode#46).

## v0.2.50

* Enable per-workspace override of `MSBuildExtensionsPath` and `MSBuildExtensionsPath32` (tintoy/msbuild-project-tools-vscode#35).

## v0.2.49

*  Prevent "dotnet --info" hanging when its output is larger than the process STDOUT buffer (tintoy/msbuild-project-tools-vscode#42).

## v0.2.47

* Improvements to logging during startup (tintoy/msbuild-project-tools-vscode#42).

## v0.2.46

* Log configured package sources when initialising a project document (tintoy/msbuild-project-tools-vscode#44).

## v0.2.45

* Handle localised output from `dotnet --info` (tintoy/msbuild-project-tools-vscode#43).

## v0.2.44

* Fix bug in parsing of extension settings.

## v0.2.43

* Optionally provide suggestions for packages from local (file-based) package sources (tintoy/msbuild-project-tools-server#9).

## v0.2.42

* Initial support for flattened (path-based) extension settings (tintoy/msbuild-project-tools-server#7).
* Start removing file-system hyperlinks from hover tooltips, since VS Code no longer renders them correctly.

## v0.2.41

* Use latest stable version of `NuGet.Configuration` to add support for encrypted credentials in `NuGet.config` (tintoy/msbuild-project-tools-vscode#39).

## v0.2.39

* Further improvements to log output (especially for project-load failures; exceptions from invalid project XML are only logged when configured log level is Debug or Verbose).

## v0.2.38

* Reduce size of VSIX package (tintoy/msbuild-project-tools-server#37).
* Improve log output (especially for project-load failures).

## v0.2.37

* Support overriding of MSBuild SDKs path via environment variable (tintoy/msbuild-project-tools-server#5).

## v0.2.36

* Bug-fix: ArgumentException (parameter name: itemType) when requesting completions on root `Project` element (tintoy/msbuild-project-tools-server#5).

## v0.2.35

* Produce cleaner stack-traces using Demystifier.

## v0.2.34

* Display help and documentation links for well-known MSBuild XML elements (tintoy/msbuild-project-tools-server#5).

## v0.2.33

* Correctly handle parsing of MSBuild expressions where the root expression is an unquoted string (i.e. composite expression including one or more string-literal text sequences).

## v0.2.32

* Expression support is no longer experimental!

## v0.2.31

* Ensure package Ids and version appear before other completion types in `PackageReference` elements / attributes.

## v0.2.30

* Add completion for `IsPackable` property.

## v0.2.29

* Bug-fix: Language server process fails to terminate correctly on Linux (tintoy/msbuild-project-tools-vscode#36).

## v0.2.28

* Add completion for `LangVersion` property.
* Improve metadata completions for `Content` items.
* Wait for Exit notification before terminating server process (tintoy/msbuild-project-tools-vscode#36).

## v0.2.27

* LSP library's logging now uses configured logging level.

## v0.2.26

* Implement completion for XML comments.

## v0.2.25

* Implement completion for top-level `<Import>` element.

## v0.2.24

* Make ASP.NET core snippets version-specific by @doggy8088 (tintoy/msbuild-project-tools-vscode#32).
* Implement default value(s) for well-known property completions (tintoy/msbuild-project-tools-vscode#31).

## v0.2.23

* Use latest version of OmniSharp LSP libraries (improves stability and diagnostic capabilities).

## v0.2.22

* Improve MSBuild snippets by @doggy8088 (tintoy/msbuild-project-tools-vscode#30).

## v0.2.21

* Add MSBuild snippets by @doggy8088 (tintoy/msbuild-project-tools-vscode#28).

## v0.2.20

* Log errors encountered while warming up NuGet client as Verbose instead of Error (tintoy/msbuild-project-tools-server#2).

## v0.2.19

* Bug-fix: Completions don't always work correctly in .props files (tintoy/msbuild-project-tools-vscode#27).
* Use latest OmniSharp LSP packages.

## v0.2.18

* Use v15.5.x of MSBuild packages (tintoy/msbuild-project-tools-server#1).

## v0.2.17

* Add completions for item elements.
* Split out language server from VS Code extension.
* Never auto-show output window on messages from language server (tintoy/msbuild-project-tools-vscode#25).

## v0.2.16

* Bug-fix: language server does not correctly report server capabilities when first initialised (tintoy/msbuild-project-tools-vscode#22).

## v0.2.15

* Add support for passing language service configuration in `InitializeParams.InitializationOptions` (tintoy/msbuild-project-tools-vscode#17).

## v0.2.14

* Offer element completions, when appropriate, in whitespace or element text (tintoy/msbuild-project-tools-vscode#15).
* Improve completion behaviour.
* Improve performance of element and attribute completions for tasks in `Target` elements.

## v0.2.13

* Bug-fix: attribute completions are erroneously offered when creating a new element under an `ItemGroup` element (tintoy/msbuild-project-tools-vscode#21).

## v0.2.12

* Simplify extension / language-service configuration schema.  
  The extension will automatically upgrade settings in the legacy format (i.e. ones without `'schemaVersion': 1`), but now ignores the old `msbuildProjectFileTools` configuration section.
* Bug-fix: completions for item metadata expressions being offered when only completions for item group expressions should be offered.
* Bug-fix: `NullReferenceException` when listing completions for item group expressions.
* Bug-fix: restore missing hover tooltip for SDK-style project import.
* Bug-fix: metadata names in unused item groups are always named "Identity".

## v0.2.11

* Diagnostics indicating invalid project contents or XML now have a range covering the whole element or attribute (where possible).

## v0.2.10

* Bug-fix: Extension won't load, after changes for tintoy/msbuild-project-tools-vscode#18, if no configuration was specified (restore configuration defaults).

## v0.2.9

* Add command (`NuGet: toggle pre-release`) to toggle NuGet pre-release packages and package versions on / off (tintoy/msbuild-project-tools-vscode#18).

## v0.2.8

* _Experimental:_ Add completions for task elements based on task types declared in the project.
* _Experimental:_ Add completions for task attributes based on task types declared in the project.
* More testing on MacOS and Linux.
* _Experimental:_ Parsing of MSBuild item transform expressions.
* _Experimental:_ Add experimental feature flag (`empty-completion-lists`) to enable returning empty completion lists rather than null  
  Fixes tintoy/msbuild-project-tools-vscode#17.  
  We can't do this by default because our extension depends on VSCode's behaviour when null is returned vs an empty completion list (when null is returned, no completion list is displayed; when an empty completion list is returned, purely-textual completions are displayed based on current file contents).  
  This behaviour is mainly to support clients other than VSCode (e.g. aCute).

## v0.2.7

* Add setting to control which types of objects from the current projects are included when offering completions.
* _Experimental:_ Add completions for qualified and unqualified item metadata expressions (`%(XXX.YYY)` and `%(YYY)`).

## v0.2.6

* Bug-fix: attribute completions should be available on elements that don't currently have any attributes.
* Bug-fix: go-to-definition should also work for regular-style project imports (not just SDK-style imports).
* _Experimental:_ Add completions for MSBuild property and item expressions (`$(XXX)` and `@(XXX)`).

## v0.2.4

* Bug-fix: missing completions for top-level elements (e.g. `<PropertyGroup>`, `<ItemGroup>`, `<Target>`).
* Improve help for well-known items and their metadata.
* Bug-fix for tintoy/msbuild-project-tools-vscode#11 (should not fail on non-standard file extension).

## v0.2.3

* Add help for well-known elements, attributes, properties, and item types from `MSBuild.*.xsd` to improve completions and tooltips-on-hover.
* Improve completions for attributes that refer to target names.

## v0.2.2

* Add completions for attributes that refer to target names.

## v0.2.1

* Add completions for top-level elements (e.g. `<PropertyGroup>`, `<ItemGroup>`, `<Target>`).
* Add completions for property elements (both common and locally-defined).
* Improve language-service internals (more consistently accurate comprehension of project contents).

## v0.2.0

* Improved completions:
  * Add completions for `PackageReference` and `DotNetCliToolReference`.
  * Add completions for common item attributes.
  * Add completions for property `Condition` elements.
  * Support for logging to [Seq](https://getseq.net/).  
    Only useful if you're hacking on the language service itself.

## v0.1.12

* Sort package versions in descending order for classic completion provider, too.  
  If you prefer the old behaviour, you can set `msbuildProjectTools.nuget.newestVersionsFirst` to `false`.

## v0.1.11

* Sort package versions in descending order.  
  If you prefer the old behaviour, you can set `msbuildProjectTools.nuget.newestVersionsFirst` to `false`.

## v0.1.10

* Improve tooltip content when hovering on MSBuild XML.
* Enable jumping from PackageReference element to package on NuGet.org.

## v0.1.9

* Add specific hover tooltip for Condition attributes.

## v0.1.8

* Add basic syntax-highlighting for expressions in MSBuild projects (currently only supported in attribute values).
* Improve delay on first completion of PackageReference by asynchronously warming up the NuGet client.

## v0.1.7

* Add configuration setting to disable tooltip-on-hover.
* Add configuration setting to control logging verbosity.

## v0.1.6

* Actually enable the language server by default (sorry about that).

## v0.1.5

* Language server is now enabled by default.
* Improve calculation of line / column offsets.

## v0.1.4

* Provide intellisense for regular-style and SDK-style imports whose conditions evaluate to false.
* Respect the user's nominated version of the .NET Core tooling to use when loading master projects (equivalent to running `dotnet --version` in the solution directory, this respects `global.json` if present).

## v0.1.3

* Provide intellisense for items whose conditions evaluate to `false`.
* Show information about conditions on hover for items and properties.

## v0.1.2

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
