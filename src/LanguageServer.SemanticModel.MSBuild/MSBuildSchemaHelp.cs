using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Help content for objects in the MSBuild XML schema.
    /// </summary>
    public static class MSBuildSchemaHelp
    {
        /// <summary>
        ///     The names of well-known MSBuild properties.
        /// </summary>
        public static IEnumerable<string> WellKnownPropertyNames => Properties.Keys;

        /// <summary>
        ///     The names of well-known MSBuild item types.
        /// </summary>
        public static IEnumerable<string> WellKnownItemTypes => Items.Keys.Where(
            itemType => itemType[0] != '*'
            &&
            itemType.IndexOf('.') == -1
        );

        /// <summary>
        ///     The qualified names of well-known MSBuild item metadata.
        /// </summary>
        public static IEnumerable<string> WellKnownItemMetadata => Items.Keys.Where(
            itemType => itemType.IndexOf('.') != -1
        );

        /// <summary>
        ///     Get help content for the specified element.
        /// </summary>
        /// <param name="elementName">
        ///     The element name.
        /// </param>
        /// <returns>
        ///     The element help content, or <c>null</c> if no content is available for it.
        /// </returns>
        public static string ForElement(string elementName)
        {
            if (String.IsNullOrWhiteSpace(elementName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'elementName'.", nameof(elementName));

            string helpKey = elementName;
            if (Root.TryGetValue(helpKey, out string help))
                return help;

            return null;
        }

        /// <summary>
        ///     Get help content for the specified attribute.
        /// </summary>
        /// <param name="elementName">
        ///     The element name.
        /// </param>
        /// <param name="attributeName">
        ///     The attribute name.
        /// </param>
        /// <returns>
        ///     The attribute help content, or <c>null</c> if no content is available for it.
        /// </returns>
        public static string ForAttribute(string elementName, string attributeName)
        {
            if (String.IsNullOrWhiteSpace(elementName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'elementName'.", nameof(elementName));

            string helpKey = String.Format("{0}.{1}", elementName, attributeName);
            if (Root.TryGetValue(helpKey, out string help))
                return help;

            return null;
        }

        /// <summary>
        ///     Get help content for a well-known MSBuild property.
        /// </summary>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <returns>
        ///     The property help content, or <c>null</c> if no content is available for it.
        /// </returns>
        public static string ForProperty(string propertyName)
        {
            if (String.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'propertyName'.", nameof(propertyName));

            string helpKey = propertyName;
            if (Properties.TryGetValue(helpKey, out string help))
                return help;

            return null;
        }

        /// <summary>
        ///     Get help content for a well-known MSBuild item type.
        /// </summary>
        /// <param name="itemType">
        ///     The item type (e.g. Compile, Content).
        /// </param>
        /// <returns>
        ///     The item help content, or <c>null</c> if no content is available for it.
        /// </returns>
        public static string ForItemType(string itemType)
        {
            if (String.IsNullOrWhiteSpace(itemType))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'itemName'.", nameof(itemType));

            string helpKey = itemType;
            if (Items.TryGetValue(helpKey, out string help))
                return help;

            return null;
        }

        /// <summary>
        ///     Get help content for a well-known MSBuild item type's metadata.
        /// </summary>
        /// <param name="itemType">
        ///     The item type (e.g. Compile, Content).
        /// </param>
        /// <param name="metadataName">
        ///     The name of the item metadata.
        /// </param>
        /// <returns>
        ///     The item metadata help content, or <c>null</c> if no content is available for it.
        /// </returns>
        public static string ForItemMetadata(string itemType, string metadataName)
        {
            if (String.IsNullOrWhiteSpace(metadataName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'metadataName'.", nameof(metadataName));

            string help;

            string helpKey = String.Format("{0}.{1}", itemType, metadataName);
            if (Items.TryGetValue(helpKey, out help))
                return help;

            helpKey = String.Format("*.{0}", metadataName);
            if (Items.TryGetValue(helpKey, out help))
                return help;

            return null;
        }

        /// <summary>
        ///     Help content for root elements and attributes, keyed by "Element" or "Element.Attribute".
        /// </summary>
        /// <remarks>
        ///     Extracted from MSBuild.*.xsd
        /// </remarks>
        static readonly Dictionary<string, string> Root = new Dictionary<string, string>
        {
            ["Choose"] = "Groups When and Otherwise elements",
            ["Choose.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["GenericProperty.Condition"] = "Optional expression evaluated to determine whether the property should be evaluated",
            ["GenericProperty.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["Import"] = "Declares that the contents of another project file should be inserted at this location",
            ["Import.Condition"] = "Optional expression evaluated to determine whether the import should occur",
            ["Import.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["Import.MinimumVersion"] = "Optional expression used to specify the minimum SDK version required by the referring import",
            ["Import.Project"] = "Project file to import",
            ["Import.Sdk"] = "Name of the SDK which contains the project file to import",
            ["Import.Version"] = "Optional expression used to specify the version of the SDK referenced by this import",
            ["ImportGroup"] = "Groups import definitions",
            ["ImportGroup.Condition"] = "Optional expression evaluated to determine whether the ImportGroup should be used",
            ["ImportGroup.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["ItemDefinitionGroup"] = "Groups item metadata definitions",
            ["ItemDefinitionGroup.Condition"] = "Optional expression evaluated to determine whether the ItemDefinitionGroup should be used",
            ["ItemDefinitionGroup.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["ItemGroup"] = "Groups item list definitions",
            ["ItemGroup.Condition"] = "Optional expression evaluated to determine whether the ItemGroup should be used",
            ["ItemGroup.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["OnError"] = "Specifies targets to execute in the event of a recoverable error",
            ["OnError.Condition"] = "Optional expression evaluated to determine whether the targets should be executed",
            ["OnError.ExecuteTargets"] = "Semicolon separated list of targets to execute",
            ["OnError.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["Otherwise"] = "Groups PropertyGroup and/or ItemGroup elements that are used if no Conditions on sibling When elements evaluate to true",
            ["ParameterGroup"] = "Groups parameters that are part of an inline task definition.",
            ["Project"] = "An MSBuild Project",
            ["Project.DefaultTargets"] = "Optional semi-colon separated list of one or more targets that will be built if no targets are otherwise specified",
            ["Project.InitialTargets"] = "Optional semi-colon separated list of targets that should always be built before any other targets",
            ["Project.Sdk"] = "Optional string describing the MSBuild SDK(s) this project should be built with",
            ["Project.ToolsVersion"] = "Optional string describing the toolset version this project should normally be built with",
            ["ProjectExtensions"] = "Optional section used by MSBuild hosts, that may contain arbitrary XML content that is ignored by MSBuild itself",
            ["PropertyGroup"] = "Groups property definitions",
            ["PropertyGroup.Condition"] = "Optional expression evaluated to determine whether the PropertyGroup should be used",
            ["PropertyGroup.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["SimpleItem.Condition"] = "Optional expression evaluated to determine whether the items should be evaluated",
            ["SimpleItem.Exclude"] = "Semicolon separated list of files (wildcards are allowed) or other item names to exclude from the Include list",
            ["SimpleItem.Include"] = "Semicolon separated list of files (wildcards are allowed) or other item names to include in this item list",
            ["SimpleItem.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["SimpleItem.Remove"] = "Semicolon separated list of files (wildcards are allowed) or other item names to remove from the existing list contents",
            ["SimpleItem.Update"] = "Semicolon separated list of files (wildcards are allowed) or other item names to be updated with the metadata from contained in this xml element",
            ["StringProperty.Condition"] = "Optional expression evaluated to determine whether the property should be evaluated",
            ["StringProperty.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["Target"] = "Groups tasks into a section of the build process",
            ["Target.AfterTargets"] = "Optional semi-colon separated list of targets that this target should run after.",
            ["Target.BeforeTargets"] = "Optional semi-colon separated list of targets that this target should run before.",
            ["Target.Condition"] = "Optional expression evaluated to determine whether the Target and the targets it depends on should be run",
            ["Target.DependsOnTargets"] = "Optional semi-colon separated list of targets that should be run before this target",
            ["Target.Inputs"] = "Optional semi-colon separated list of files that form inputs into this target. Their timestamps will be compared with the timestamps of files in Outputs to determine whether the Target is up to date",
            ["Target.KeepDuplicateOutputs"] = "Optional expression evaluated to determine whether duplicate items in the Target's Returns should be removed before returning them. The default is not to eliminate duplicates.",
            ["Target.Label"] = "Optional expression. Used to identify or order system and user elements",
            ["Target.Name"] = "Name of the target",
            ["Target.Outputs"] = "Optional semi-colon separated list of files that form outputs into this target. Their timestamps will be compared with the timestamps of files in Inputs to determine whether the Target is up to date",
            ["Target.Returns"] = "Optional expression evaluated to determine which items generated by the target should be returned by the target. If there are no Returns attributes on Targets in the file, the Outputs attributes are used instead for this purpose.",
            ["Task.Architecture"] = "Defines the bitness of the task if it must be run specifically in a 32bit or 64bit process. If not specified, it will run with the bitness of the build process.  If there are multiple tasks defined in UsingTask with the same name but with different Architecture attribute values, the value of the Architecture attribute specified here will be used to match and select the correct task",
            ["Task.Condition"] = "Optional expression evaluated to determine whether the task should be executed",
            ["Task.ContinueOnError"] = "Optional boolean indicating whether a recoverable task error should be ignored. Default false",
            ["Task.Output"] = "Optional element specifying a specific task output to be gathered",
            ["Task.Output.Condition"] = "Optional expression evaluated to determine whether the output should be gathered",
            ["Task.Output.ItemName"] = "Optional name of an item list to put the gathered outputs into. Either ItemName or PropertyName must be specified",
            ["Task.Output.PropertyName"] = "Optional name of a property to put the gathered output into. Either PropertyName or ItemName must be specified",
            ["Task.Output.TaskParameter"] = "Task parameter to gather. Matches the name of a .NET Property on the task class that has an [Output] attribute",
            ["Task.Runtime"] = "Defines the .NET runtime of the task. This must be specified if the task must run on a specific version of the .NET runtime. If not specified, the task will run on the runtime being used by the build process. If there are multiple tasks defined in UsingTask with the same name but with different Runtime attribute values, the value of the Runtime attribute specified here will be used to match and select the correct task",
            ["UsingTask"] = "Defines the assembly containing a task's implementation, or contains the implementation itself.",
            ["UsingTask.Architecture"] = "Defines the architecture of the task host that this task should be run in.  Currently supported values:  x86, x64, CurrentArchitecture, and * (any).  If Architecture is not specified, either the task will be run within the MSBuild process, or the task host will be launched using the architecture of the parent MSBuild process",
            ["UsingTask.AssemblyFile"] = "Optional path to assembly containing the task. Either AssemblyName or AssemblyFile must be used",
            ["UsingTask.AssemblyName"] = "Optional name of assembly containing the task. Either AssemblyName or AssemblyFile must be used",
            ["UsingTask.Condition"] = "Optional expression evaluated to determine whether the declaration should be evaluated",
            ["UsingTask.Runtime"] = "Defines the .NET runtime version of the task host that this task should be run in.  Currently supported values:  CLR2, CLR4, CurrentRuntime, and * (any).  If Runtime is not specified, either the task will be run within the MSBuild process, or the task host will be launched using the runtime of the parent MSBuild process",
            ["UsingTask.TaskFactory"] = "Name of the task factory class in the assembly",
            ["UsingTask.TaskName"] = "Name of task class in the assembly",
            ["UsingTaskBody"] = "Contains the inline task implementation. Content is opaque to MSBuild.",
            ["UsingTaskBody.Evaluate"] = "Whether the body should have properties expanded before use. Defaults to false.",
            ["When"] = "Groups PropertyGroup and/or ItemGroup elements",
            ["When.Condition"] = "Optional expression evaluated to determine whether the child PropertyGroups and/or ItemGroups should be used",
        };

        /// <summary>
        ///     Help content for property elements, keyed by property name.
        /// </summary>
        /// <remarks>
        ///     Extracted from MSBuild.*.xsd
        /// </remarks>
        static readonly Dictionary<string, string> Properties = new Dictionary<string, string>
        {
            ["AllowLocalNetworkLoopback"] = "Flag indicating whether to allow local network loopback.",
            ["AllowUnsafeBlocks"] = "Allow unsafe code blocks.",
            ["AppDesignerFolder"] = "Name of folder for Application Designer",
            ["ApplicationRevision"] = "integer",
            ["ApplicationVersion"] = "Matches the expression \"\\d\\.\\d\\.\\d\\.(\\d|\\*)\"",
            ["AppxAutoIncrementPackageRevision"] = "Flag indicating whether to auto-increment package revision.",
            ["AppxBundle"] = "Flag indicating whether packaging targets will produce an app bundle.",
            ["AppxBundleAutoResourcePackageQualifiers"] = "'|'-delimited list of resource qualifiers which will be used for automatic resource pack splitting.",
            ["AppxBundleDir"] = "Full path to a folder where app bundle will be produced.",
            ["AppxBundleFolderSuffix"] = "Suffix to append to app bundle folder.",
            ["AppxBundleMainPackageFileMapGeneratedFilesListPath"] = "Full path to a log file containing a list of generated files during generation of main package file map.",
            ["AppxBundleMainPackageFileMapIntermediatePath"] = "Full path to an intermediate main package file map.",
            ["AppxBundleMainPackageFileMapIntermediatePrefix"] = "Prefix used for intermediate main package resources .pri and .map.txt files.",
            ["AppxBundleMainPackageFileMapIntermediatePriPath"] = "Full path to an intermediate main package .pri file.",
            ["AppxBundleMainPackageFileMapPath"] = "Full path to a main package file map.",
            ["AppxBundleMainPackageFileMapPrefix"] = "Prefix used for main package resources .pri and .map.txt files.",
            ["AppxBundleMainPackageFileMapSuffix"] = "Suffix used before extension of resource map files.",
            ["AppxBundlePlatforms"] = "'|'-delimited list of platforms which will be included in an app bundle.",
            ["AppxBundlePriConfigXmlForMainPackageFileMapFileName"] = "Full path to the priconfig.xml file used for generating main package file map.",
            ["AppxBundlePriConfigXmlForSplittingFileName"] = "Full path to the priconfig.xml file used for splitting resource packs.",
            ["AppxBundleProducingPlatform"] = "A platform which will be used to produce an app bundle.",
            ["AppxBundleResourcePacksProducingPlatform"] = "A platform which will be used to produce resource packs for an app bundle.",
            ["AppxBundleSplitResourcesGeneratedFilesListPath"] = "Full path to a log file containing a list of generated files during resource splitting.",
            ["AppxBundleSplitResourcesPriPath"] = "Full path to split resources .pri file.",
            ["AppxBundleSplitResourcesPriPrefix"] = "Prefix used for split resources .pri and .map.txt files.",
            ["AppxBundleSplitResourcesQualifiersPath"] = "Full path to a log file containing a detected qualifiers during resource splitting.",
            ["AppxCopyLocalFilesOutputGroupIncludeXmlFiles"] = "Flag indicating whether CopyLocal files group should include XML files.",
            ["AppxCreatePriFilesForPortableLibrariesAdditionalMakepriExeParameters"] = "Additional parameters to pass to makepri.exe when generating PRI file for a portable library.",
            ["AppxDefaultHashAlgorithmId"] = "Default hash algorithm ID, used for signing an app package.",
            ["AppxDefaultResourceQualifiers"] = "'|'-delimited list of key=value pairs representing default resource qualifiers.",
            ["AppxExpandPriContentAdditionalMakepriExeParameters"] = "Additional parameters to pass to makepri.exe when extracting payload file names.",
            ["AppxFilterOutUnusedLanguagesResourceFileMaps"] = "Flag indicating whether to filter out unused language resource file maps.",
            ["AppxGeneratePriEnabled"] = "Flag indicating whether to generate resource index files (PRI files) during packaging.",
            ["AppxGenerateProjectPriFileAdditionalMakepriExeParameters"] = "Additional parameters to pass to makepri.exe when generating project PRI file.",
            ["AppxHarvestWinmdRegistration"] = "Flag indicating whether to enable harvesting of WinMD registration information.",
            ["AppxLayoutDir"] = "Full path to the folder where package layout will be prepared when producing an app bundle.",
            ["AppxLayoutFolderName"] = "Name of the folder where package layout will be prepared when producing an app bundle.",
            ["AppxMSBuildTaskAssembly"] = "Full path to packaging build tasks assembly.",
            ["AppxMSBuildToolsPath"] = "Full path to a folder containing packaging build targets and tasks assembly.",
            ["AppxOSMaxVersionTested"] = "Targeted maximum OS version tested.",
            ["AppxOSMaxVersionTestedReplaceManifestVersion"] = "Flag indicating whether maximum OS version tested in app manifest should be replaced.",
            ["AppxOSMinVersion"] = "Targeted minimum OS version.",
            ["AppxOSMinVersionReplaceManifestVersion"] = "Flag indicating whether minimum OS version in app manifest should be replaced.",
            ["AppxPackage"] = "Flag marking current project as capable of being packaged as an app package.",
            ["AppxPackageAllowDebugFrameworkReferencesInManifest"] = "Flag indicating whether to allow inclusion of debug framework references in an app manifest.",
            ["AppxPackageArtifactsDir"] = "Additional qualifier to append to AppxPackageDir.",
            ["AppxPackageDir"] = "Full path to a folder where app packages will be saved.",
            ["AppxPackageDirName"] = "Name of the folder where app packages are produced.",
            ["AppxPackageFileMap"] = "Full path to app package file map.",
            ["AppxPackageIncludePrivateSymbols"] = "Flag indicating whether to include private symbols in symbol packages.",
            ["AppxPackageName"] = "Name of the app package to generate.",
            ["AppxPackageOutput"] = "Full path to the app package file.",
            ["AppxPackageRecipe"] = "Full path to the app package recipe.",
            ["AppxPackageSigningEnabled"] = "Flag indicating whether to enable signing of app packages.",
            ["AppxPackageTestDir"] = "Name of the folder where test app packages will be copied",
            ["AppxPackageValidationEnabled"] = "Flag indicating whether to enable validation of app packages.",
            ["AppxPackagingInfoFile"] = "Full path to the packaging info file which will contain paths to produced packages.",
            ["AppxPrependPriInitialPath"] = "Flag indicating whether to enable prepending initial path when indexing RESW and RESJSON files in class libraries.",
            ["AppxPriConfigXmlDefaultSnippetPath"] = "Path to an XML file containing default element for priconfi.xml file.",
            ["AppxPriConfigXmlPackagingSnippetPath"] = "Path to an XML file containing packaging element for priconfi.xml file.",
            ["AppxPriInitialPath"] = "Initial path when indexing RESW and RESJSON files in class libraries.",
            ["AppxSkipUnchangedFiles"] = "Flag indicating whether to skip unchanged files when copying files during creation of app packages.",
            ["AppxStoreContainer"] = "Name of the app store container to generate.",
            ["AppxStrictManifestValidationEnabled"] = "Flag indicating whether to enable strict manifest validation.",
            ["AppxSymbolPackageEnabled"] = "Flag indicating whether to generate a symbol package when an app package is created.",
            ["AppxSymbolPackageOutput"] = "Full path to the app symbol package file.",
            ["AppxSymbolStrippedDir"] = "Full path to a directory where stripped PDBs will be stored.",
            ["AppxTestLayoutEnabled"] = "Flag indicating whether to create test layout when an app package is created.",
            ["AppxUseHardlinksIfPossible"] = "Flag indicating whether to use hard links if possible when copying files during creation of app packages.",
            ["AppxValidateAppxManifest"] = "Flag indicating whether to validate app manifest.",
            ["AppxValidateStoreManifest"] = "Flag indicating whether to validate store manifest.",
            ["AssemblyKeyContainerName"] = "The name of the CAPI container that holds the strong-naming key used to sign the output assembly.",
            ["AssemblyKeyProviderName"] = "The name of the CAPI provider that provides the strong-naming key used to sign the output assembly.",
            ["AssemblyName"] = "Name of output assembly",
            ["AssemblyOriginatorKeyFile"] = "The path of the strong-naming key (.snk) file used to sign the output assembly.",
            ["AssemblyTitle"] = "Description for the assembly manifest",
            ["AssemblyVersion"] = "Numeric value of the version for the assembly manifest in the format major.minor.patch (e.g. 2.4.0)",
            ["Authors"] = "A comma-separated list of NuGet packages authors",
            ["AutoGenerateBindingRedirects"] = "Indicates whether BindingRedirect elements should be automatically generated for referenced assemblies.",
            ["AutoIncrementPackageRevision"] = "Flag indicating whether to enable auto increment of an app package revision.",
            ["AutorunEnabled"] = "boolean",
            ["BootstrapperComponentsLocation"] = "HomeSite, Relative, or Absolute",
            ["BootstrapperEnabled"] = "boolean",
            ["CodeAnalysisAdditionalOptions"] = "Additional options to pass to the Code Analysis command line tool.",
            ["CodeAnalysisApplyLogFileXsl"] = "Indicates whether to apply the XSL style sheet specified in $(CodeAnalysisLogFileXsl) to the Code Analysis report. This report is specified in $(CodeAnalysisLogFile). The default is false.",
            ["CodeAnalysisConsoleXsl"] = "Path to the XSL style sheet that will be applied to the Code Analysis console output. The default is an empty string (''), which causes Code Analysis to use its default console output.",
            ["CodeAnalysisCulture"] = "Culture to use for Code Analysis spelling rules, for example, 'en-US' or 'en-AU'. The default is the current user interface language for Windows.",
            ["CodeAnalysisFailOnMissingRules"] = "Indicates whether Code Analysis should fail if a rule or rule set is missing. The default is false.",
            ["CodeAnalysisForceOutput"] = "Indicates whether Code Analysis generates a report file, even when there are no active warnings or errors. The default is true.",
            ["CodeAnalysisGenerateSuccessFile"] = "Indicates whether Code Analysis generates a '$(CodeAnalysisInputAssembly).lastcodeanalysissucceeded' file in the output folder when no build-breaking errors occur. The default is true.",
            ["CodeAnalysisIgnoreBuiltInRules"] = "Indicates whether Code Analysis will ignore the default rule directories when searching for rules. The default is false.",
            ["CodeAnalysisIgnoreBuiltInRuleSets"] = "Indicates whether Code Analysis will ignore the default rule set directories when searching for rule sets. The default is false.",
            ["CodeAnalysisIgnoreGeneratedCode"] = "Indicates whether Code Analysis should fail silently when it analyzes invalid assemblies, such as those without managed code. The default is true.",
            ["CodeAnalysisIgnoreInvalidTargets"] = "Indicates whether Code Analysis should silently fail when analyzing invalid assemblies, such as those without managed code. The default is true.",
            ["CodeAnalysisInputAssembly"] = "Path to the assembly to be analyzed by Code Analysis. The default is '$(OutDir)$(TargetName)$(TargetExt)'.",
            ["CodeAnalysisLogFile"] = "Path to the output file for the Code Analysis report. The default is '$(CodeAnalysisInputAssembly).CodeAnalysisLog.xml'.",
            ["CodeAnalysisLogFileXsl"] = "Path to the XSL style sheet to reference in the Code Analysis output report. This report is specified in $(CodeAnalysisLogFile). The default is an empty string ('').",
            ["CodeAnalysisModuleSuppressionsFile"] = "Name of the file, without the path, where Code Analysis project-level suppressions are stored. The default is 'GlobalSuppressions$(DefaultLanguageSourceExtension)'.",
            ["CodeAnalysisOutputToConsole"] = "Indicates whether to output Code Analysis warnings and errors to the console. The default is false.",
            ["CodeAnalysisOverrideRuleVisibilities"] = "Indicates whether to run all overridable Code Analysis rules against all targets. This will cause specific rules, such as those within the Design and Naming categories, to run against both public and internal APIs, instead of only public APIs. The default is false.",
            ["CodeAnalysisPath"] = "Path to the Code Analysis installation folder. The default is '$(VSINSTALLDIR)\\Team Tools\\Static Analysis Tools\\FxCop'.",
            ["CodeAnalysisPlatformPath"] = "Path to the .NET Framework folder that contains platform assemblies, such as mscorlib.dll and System.dll. The default is an empty string ('').",
            ["CodeAnalysisProject"] = "Path to the Code Analysis project (*.fxcop) to load. The default is an empty string ('').",
            ["CodeAnalysisQuiet"] = "Indicates whether to suppress all Code Analysis console output other than errors and warnings. This applies when $(CodeAnalysisOutputToConsole) is true. The default is false.",
            ["CodeAnalysisRuleAssemblies"] = "Semicolon-separated list of paths either to Code Analysis rule assemblies or to folders that contain Code Analysis rule assemblies. The paths are in the form '[+|-][!][file|folder]', where '+' enables all rules in rule assembly, '-' disables all rules in rule assembly, and '!' causes all rules in rule assembly to be treated as errors. For example '+D:\\Projects\\Rules\\NamingRules.dll;+!D:\\Projects\\Rules\\SecurityRules.dll'. The default is '$(CodeAnalysisPath)\\Rules'.",
            ["CodeAnalysisRuleDirectories"] = "Semicolon-separated list of directories in which to search for rules when resolving a rule set. The default is '$(CodeAnalysisPath)\\Rules' unless the CodeAnalysisIgnoreBuiltInRules property is set to true.",
            ["CodeAnalysisRules"] = "Semicolon-separated list of Code Analysis rules. The rules are in the form '[+|-][!]Category#CheckId', where '+' enables the rule, '-' disables the rule, and '!' causes the rule to be treated as an error. For example, '-Microsoft.Naming#CA1700;+!Microsoft.Naming#CA1701'. The default is an empty string ('') which enables all rules.",
            ["CodeAnalysisRuleSet"] = "A .ruleset file which contains a list of rules to run during analysis. The string can be a full path, a path relative to the project file, or a file name. If a file name is specified, the CodeAnalysisRuleSetDirectories property will be searched to find the file. The default is an empty string ('').",
            ["CodeAnalysisRuleSetDirectories"] = "Semicolon-separated list of directories in which to search for rule sets. The default is '$(VSINSTALLDIR)\\Team Tools\\Static Analysis Tools\\Rule Sets' unless the CodeAnalysisIgnoreBuiltInRuleSets property is set to true.",
            ["CodeAnalysisSaveMessagesToReport"] = "Comma-separated list of the type ('Active', 'Excluded', or 'Absent') of warnings and errors to save to the output report file. The default is 'Active'.",
            ["CodeAnalysisSearchGlobalAssemblyCache"] = "Indicates whether Code Analysis should search the Global Assembly Cache (GAC) for missing references that are encountered during analysis. The default is true.",
            ["CodeAnalysisSummary"] = "Indicates whether to output a Code Analysis summary to the console after analysis. The default is false.",
            ["CodeAnalysisTimeout"] = "The time, in seconds, that Code Analysis should wait for analysis of a single item to complete before it aborts analysis. Specify 0 to cause Code Analysis to wait indefinitely. The default is 120.",
            ["CodeAnalysisTreatWarningsAsErrors"] = "Indicates whether to treat all Code Analysis warnings as errors. The default is false.",
            ["CodeAnalysisUpdateProject"] = "Indicates whether to update the Code Analysis project (*.fxcop) specified in $(CodeAnalysisProject). This applies when there are changes during analysis. The default is false.",
            ["CodeAnalysisUseTypeNameInSuppression"] = "Indicates whether to include the name of the rule when Code Analysis emits a suppression. The default is true.",
            ["CodeAnalysisVerbose"] = "Indicates whether to output verbose Code Analysis diagnostic info to the console. The default is false.",
            ["Company"] = "Company name for the assembly manifest",
            ["Configuration"] = "The project build configuration (e.g. Debug, Release).",
            ["Copyright"] = "Copyright details for the NuGet package",
            ["CreateWebPageOnPublish"] = "boolean",
            ["DebugSymbols"] = "Whether to emit symbols (boolean)",
            ["DebugType"] = "none, pdbonly, or full",
            ["DefaultLanguage"] = "Default resource language.",
            ["DefineConstants"] = "Compile-time constants to be defined (e.g. DEBUG, TRACE).",
            ["DefineDebug"] = "Whether DEBUG is defined (boolean)",
            ["DefineTrace"] = "Whether TRACE is defined (boolean)",
            ["DelaySign"] = "Delay-sign the output assembly (boolean).",
            ["Description"] = "A long description of the NuGet package for UI display",
            ["DisableFastUpToDateCheck"] = "Whether Visual Studio should do its own faster up-to-date check before Building, rather than invoke MSBuild to do a possibly more accurate one. You would set this to false if you have a heavily customized build process and builds in Visual Studio are not occurring when they should.",
            ["DocumentationFile"] = "The XML documentation file to generate from doc-comments.",
            ["EnableASPDebugging"] = "Enable classic ASP debugging (boolean).",
            ["EnableASPXDebugging"] = "Enable ASP.NET Razor debugging (boolean).",
            ["EnableSigningChecks"] = "Flag indicating whether to enable signing checks during app package generation.",
            ["EnableUnmanagedDebugging"] = "Enable unmanaged debugging (boolean).",
            ["FileVersion"] = "Numeric value of the version for the assembly manifest in the format major.minor.build.revision (e.g. 2.4.0.1)",
            ["FinalAppxManifestName"] = "Path to the final app manifest.",
            ["FinalAppxPackageRecipe"] = "Full path to the final app package recipe.",
            ["FrameworkPathOverride"] = "Sets the /sdkpath switch for a VB project to the specified value",
            ["GenerateAppxPackageOnBuild"] = "Flag indicating whether to generate app package during the build.",
            ["GeneratePackageOnBuild"] = "Value indicating whether a NuGet package will be generated when the project is built",
            ["IncludeBuiltProjectOutputGroup"] = "Flag indicating whether to include primary build outputs into the app package payload.",
            ["IncludeComFilesOutputGroup"] = "Flag indicating whether to include COM files into the app package payload.",
            ["IncludeContentFilesProjectOutputGroup"] = "Flag indicating whether to include content files into the app package payload.",
            ["IncludeCopyLocalFilesOutputGroup"] = "Flag indicating whether to include files marked as 'Copy local' into the app package payload.",
            ["IncludeCopyWinmdArtifactsOutputGroup"] = "Flag indicating whether to include WinMD artifacts into the app package payload.",
            ["IncludeCustomOutputGroupForPackaging"] = "Flag indicating whether to include custom output group into the app package payload.",
            ["IncludeDebugSymbolsProjectOutputGroup"] = "Flag indicating whether to include debug symbols into the app package payload.",
            ["IncludeDocumentationProjectOutputGroup"] = "Flag indicating whether to include documentation into the app package payload.",
            ["IncludeGetResolvedSDKReferences"] = "Flag indicating whether to include resolved SDK references into the app package payload.",
            ["IncludePriFilesOutputGroup"] = "Flag indicating whether to include resource index (PRI) files into the app package payload.",
            ["IncludeSatelliteDllsProjectOutputGroup"] = "Flag indicating whether to include satellite DLLs into the app package payload.",
            ["IncludeSDKRedistOutputGroup"] = "Flag indicating whether to include SDK redist into the app package payload.",
            ["IncludeSGenFilesOutputGroup"] = "Flag indicating whether to include SGen files into the app package payload.",
            ["IncludeSourceFilesProjectOutputGroup"] = "Flag indicating whether to include source files into the app package payload.",
            ["InformationalVersion"] = "Product version of the assembly for UI display (e.g. 1.0 Beta)",
            ["InsertReverseMap"] = "Flag indicating whether to insert reverse resource map during resource index generation.",
            ["InstallFrom"] = "Web, Unc, or Disk",
            ["LayoutDir"] = "Full path to a folder with package layout.",
            ["MakeAppxExeFullPath"] = "Full path to makeappx.exe utility.",
            ["MakePriExeFullPath"] = "Full path to makepri.exe utility.",
            ["ManagedWinmdInprocImplementation"] = "Name of the binary containing managed WinMD in-proc implementation.",
            ["MapFileExtensions"] = "boolean",
            ["MinimumRequiredVersion"] = "Matches the expression \"\\d\\.\\d\\.\\d\\.\\d\"",
            ["MSBuildTreatWarningsAsErrors"] = "Indicates whether to treat all warnings as errors when building a project.",
            ["MSBuildWarningsAsErrors"] = "Indicates a semicolon delimited list of warnings to treat as errors when building a project.",
            ["MSBuildWarningsAsMessages"] = "Indicates a semicolon delimited list of warnings to treat as low importance messages when building a project.",
            ["NeutralLanguage"] = "The locale ID for the NuGet package",
            ["NoStdLib"] = "Whether standard libraries (such as mscorlib) should be referenced automatically (boolean)",
            ["NoWarn"] = "Comma separated list of disabled warnings",
            ["OpenBrowserOnPublish"] = "boolean",
            ["Optimize"] = "Should compiler optimize output (boolean)",
            ["OptionCompare"] = "Option Compare setting (Text or Binary)",
            ["OptionExplicit"] = "Should Option Explicit be set (On or Off)",
            ["OptionInfer"] = "Should Option Infer be set (On or Off)",
            ["OptionStrict"] = "Should Option Strict be set (On or Off)",
            ["OutDir"] = "Path to output folder, with trailing slash (legacy property; prefer use of \"OutputPath\" instead)",
            ["OutputPath"] = "Path to output folder, with trailing slash",
            ["OutputType"] = "Type of output to generate (WinExe, Exe, or Library)",
            ["PackageCertificateKeyFile"] = "App package certificate key file.",
            ["PackageIconUrl"] = "The URL for a 64x64 image with transparent background to use as the icon for the NuGet package in UI display",
            ["PackageId"] = "The case-insensitive NuGet package identifier, which must be unique across nuget.org or whatever gallery the NuGet package will reside in. IDs may not contain spaces or characters that are not valid for a URL, and generally follow .NET namespace rules.",
            ["PackageLicenseUrl"] = "The URL for the NuGet package's license, often shown in UI displays as well as nuget.org",
            ["PackageProjectUrl"] = "The URL for the NuGet package's home page, often shown in UI displays as well as nuget.org",
            ["PackageReleaseNotes"] = "A description of the changes made in this release of the NuGet package, often used in UI like the Updates tab of the Visual Studio Package Manager in place of the package description",
            ["PackageRequireLicenseAcceptance"] = "Value indicating whether the client must prompt the consumer to accept the NuGet package license before installing the package",
            ["PackageTags"] = "A space-delimited list of tags and keywords that describe the NuGet package and aid discoverability of NuGet packages through search and filtering mechanisms",
            ["PackageTargetFallback"] = "Allows packages using alternative monikers to be referenced in this project, which include older (e.g. dnxcore50, dotnet5.x) and Portable Class Library names.",
            ["PackageVersion"] = "Numeric value of the NuGet package version in the format major.minor.patch pattern (e.g. 1.0.1). Version numbers may include a pre-release suffix (e.g. 1.0.1-beta)",
            ["PackagingDirectoryWritesLogPath"] = "Full path to a text file containing packaging directory writes log.",
            ["PackagingFileWritesLogPath"] = "Full path to a text file containing packaging file writes log.",
            ["PdbCopyExeFullPath"] = "Full path to pdbcopy.exe utility.",
            ["PlatformSpecificBundleArtifactsListDir"] = "Full path to a folder where platform-specific bundle artifact list files are stored.",
            ["PlatformSpecificBundleArtifactsListDirName"] = "Name of the folder where platform-specific bundle artifact lists are stored.",
            ["PostBuildEvent"] = "Command line to be run at the end of build",
            ["PreBuildEvent"] = "Command line to be run at the start of build",
            ["PreserveCompilationContext"] = "Value indicating whether reference assemblies can be used in dynamic compilation",
            ["Product"] = "Product name information for the assembly manifest",
            ["ProjectPriFileName"] = "File name to use for project-specific resource index file (PRI).",
            ["ProjectPriFullPath"] = "Full path to project-specific resource index file (PRI).",
            ["ProjectPriIndexName"] = "Name of the resource index used in the generated .pri file.",
            ["ReferencePath"] = "Semicolon separated list of folders to search during reference resolution",
            ["RegisterForComInterop"] = "Register the assembly's types for COM Interop (boolean).",
            ["RepositoryType"] = "The type of the repository where the project is stored (e.g. git)",
            ["RepositoryUrl"] = "The URL for the repository where the project is stored",
            ["ResgenToolPath"] = "Full path to a folder containing resgen tool.",
            ["RootNamespace"] = "The default namespace for new code files added to the project.",
            ["RunCodeAnalysis"] = "Indicates whether to run Code Analysis during the build.",
            ["RuntimeIdentifier"] = "Runtime identifier supported by the project (e.g. win10-x64)",
            ["RuntimeIdentifiers"] = "Semicolon separated list of runtime identifiers supported by the project (e.g. win10-x64;osx.10.11-x64;ubuntu.16.04-x64)",
            ["SignAppxPackageExeFullPath"] = "Full path to signtool.exe utility.",
            ["SignAssembly"] = "Sign the output assembly, giving it a strong name (boolean).",
            ["SolutionDir"] = "The solution directory.",
            ["SolutionExt"] = "The solution file extension.",
            ["SolutionFileName"] = "The solution file name.",
            ["SolutionName"] = "The solution file name, without extension.",
            ["SolutionPath"] = "The full path to the solution file.",
            ["StartupObject"] = "Type that contains the main entry point",
            ["StoreManifestName"] = "Name of the store manifest file.",
            ["TargetFramework"] = "Framework that this project targets. Must be a Target Framework Moniker (e.g. netcoreapp1.0)",
            ["TargetFrameworks"] = "Semicolon separated list of frameworks that this project targets. Must be a Target Framework Moniker (e.g. netcoreapp1.0;net461)",
            ["TargetPlatformSdkRootOverride"] = "Full path to platform SDK root.",
            ["Title"] = "A human-friendly title of the package, typically used in UI displays as on nuget.org and the Package Manager in Visual Studio. If not specified, the package ID is used instead.",
            ["TreatWarningsAsErrors"] = "Treat compile warnings as errors (boolean).",
            ["UpdateIntervalUnits"] = "Hours, Days, or Weeks",
            ["UpdateMode"] = "Foreground or Background",
            ["UpdatePeriodically"] = "boolean",
            ["UpdateRequired"] = "boolean",
            ["UseIncrementalAppxRegistration"] = "Flag indicating whether to enable incremental registration of the app layout.",
            ["Version"] = "Numeric value of the version in the format major.minor.patch (e.g. 2.4.0)",
            ["VersionPrefix"] = "When Version is not specified, VersionPrefix represents the first fragment of the version string (e.g. 1.0.0). The syntax is VersionPrefix[-VersionSuffix].",
            ["VersionSuffix"] = "When Version is not specified, VersionSuffix represents the second fragment of the version string (e.g. beta). The syntax is VersionPrefix[-VersionSuffix].",
            ["WarningLevel"] = "integer between 0 and 4 inclusive",
            ["WarningsAsErrors"] = "Comma separated list of warning numbers to treat as errors"
        };

        /// <summary>
        ///     Help content for item types / item metadata , keyed by "ItemType" or "ItemType.MetadataName".
        /// </summary>
        /// <remarks>
        ///     Extracted from MSBuild.*.xsd
        /// </remarks>
        static readonly Dictionary<string, string> Items = new Dictionary<string, string>
        {
            ["*.Identity"] ="The item specified in the Include attribute.",
            ["*.RootDir"] ="Contains the root directory of the item.",
            ["*.Filename"] ="Contains the file name of the item, without the extension.",
            ["*.Extension"] ="Contains the file name extension of the item.",
            ["*.RelativeDir"] ="Contains the path specified in the Include attribute, up to the final slash or backslash.",
            ["*.Directory"] ="Contains the directory of the item, without the root directory.",
            ["*.RecursiveDir"] ="If the Include attribute contains the wildcard **, this metadata specifies the part of the path that replaces the wildcard.",
            ["*.ModifiedTime"] ="Contains the timestamp from the last time the item was modified.",
            ["*.FullPath"] = "Contains the full path of the item.",
            ["*.CreatedTime"] ="Contains the timestamp from the last time the item was created.",
            ["*.AccessedTime"] ="Contains the timestamp from the last time the item was last accessed.",
            ["*.CopyToOutputDirectory"] = "Copy the item to the output directory (boolean, default false).",
            ["*.CustomToolNamespace"] = "The namespace for code generated by the custom tool associated with the item.",
            ["*.DependentUpon"] = "Indicates that the item is dependent upon another item.",
            ["*.SubType"] = "The item sub-type.",
            ["*.Generator"] = "Name of any file generator that is run on this item.",
            ["*.Visible"] = "Display in the item in the user interface? (optional, boolean)",
            ["Analyzer"] = "An assembly containing diagnostic analyzers",
            ["ApplicationDefinition"] = "XAML file that contains the application definition, only one can be defined",
            ["AppxHashUri"] = "Hash algorithm URI.",
            ["AppxHashUri.Id"] = "Hash algorithm ID corresponding to given hash URI.",
            ["AppxManifest"] = "app manifest template",
            ["AppxManifestFileNameQuery"] = "XPath queries used to extract file names from the app manifest.",
            ["AppxManifestImageFileNameQuery"] = "XPath queries used to define image files in the app manifest and restrictions on them.",
            ["AppxManifestImageFileNameQuery.DescriptionID"] = "ID of description string resource describing this type of the image.",
            ["AppxManifestImageFileNameQuery.ExpectedScaleDimensions"] = "Semicolon-delimited list of expected scale dimensions in format '{scale}:{width}x{height}'.",
            ["AppxManifestImageFileNameQuery.ExpectedTargetSizes"] = "Semicolon-delimited list of expected target sizes.",
            ["AppxManifestImageFileNameQuery.MaximumFileSize"] = "Maximum file size in bytes.",
            ["AppxManifestMetadata"] = "App manifest metadata item. Can be a literal, or it can be a path to a binary to extract version from.",
            ["AppxManifestMetadata.Name"] = "Name of app manifest metadata to insert into manifest.",
            ["AppxManifestMetadata.Value"] = "Literal value of app manifest metadata to insert into manifest.",
            ["AppxManifestMetadata.Version"] = "Version to be inserted as a value of app manifest metadata to insert into manifest.",
            ["AppxManifestSchema"] = "App manifest schema file.",
            ["AppxReservedFileName"] = "Reserved file name which cannot appear in the app package.",
            ["AppxSystemBinary"] = "Name of any file which is present on the machine and should not be part of the app payload.",
            ["BaseApplicationManifest"] = "The base application manifest for the build. Contains ClickOnce security information.",
            ["CodeAnalysisDependentAssemblyPaths"] = "Additional reference assembly paths to pass to the Code Analysis command line tool.",
            ["CodeAnalysisDictionary"] = "Code Analysis custom dictionaries.",
            ["CodeAnalysisImport"] = "Code Analysis projects (*.fxcop) or reports to import.",
            ["COMFileReference"] = "Type libraries that feed into the ResolvedComreference target.",
            ["COMFileReference.WrapperTool"] = " The name of the wrapper tool that is used on the component, for example, \"tlbimp.\"",
            ["Compile"] = "Source files for compiler",
            ["Compile.AutoGen"] = "Whether file was generated from another file (boolean)",
            ["COMReference"] = "Reference to a COM component",
            ["COMReference.EmbedInteropTypes"] = "Whether the types in this reference need to embedded into the target assembly - interop assemblies only (optional, boolean)",
            ["COMReference.Guid"] = "GUID in the form {00000000-0000-0000-0000-000000000000}",
            ["COMReference.Isolated"] = "Is it isolated (boolean)",
            ["COMReference.Lcid"] = "Locale ID",
            ["COMReference.Name"] = "Friendly display name (optional)",
            ["COMReference.VersionMajor"] = "Major part of the version number",
            ["COMReference.VersionMinor"] = "Minor part of the version number",
            ["COMReference.WrapperTool"] = "Wrapper tool, such as tlbimp",
            ["Content"] = "Files that are not compiled, but may be embedded or published",
            ["Content.CopyToPublishDirectory"] = "Copy file to publish directory (optional, boolean, default false)",
            ["Content.PublishState"] = "Default, Included, Excluded, DataFile, or Prerequisite",
            ["DotNetCliToolReference"] = "The CLI tool that the user wants restored in the context of the project",
            ["DotNetCliToolReference.Version"] = "The tool version",
            ["DotNetCliToolReference.ExcludeAssets"] = "Assets to exclude from this reference",
            ["DotNetCliToolReference.IncludeAssets"] = "Assets to include from this reference",
            ["DotNetCliToolReference.PrivateAssets"] = "Assets that are private in this reference",
            ["EmbeddedResource"] = "Resources to be embedded in the generated assembly",
            ["EmbeddedResource.LastGenOutput"] = "File that was created by any file generator that was run on this item",
            ["Folder"] = "Folder on disk",
            ["Import"] = "Assemblies whose namespaces should be imported by the Visual Basic compiler",
            ["NativeReference"] = "Reference to a native manifest file, or to a file that contains a native manifest",
            ["NativeReference.HintPath"] = "Relative path to manifest file",
            ["NativeReference.Name"] = "Base name of manifest file",
            ["None"] = "Files that should have no role in the build process",
            ["None.LastGenOutput"] = null,
            ["PackageReference"] = "Reference to a package",
            ["PackageReference.Version"] = "The package version",
            ["PackageReference.ExcludeAssets"] = "Assets to exclude from this reference",
            ["PackageReference.IncludeAssets"] = "Assets to include from this reference",
            ["PackageReference.PrivateAssets"] = "Assets that are private in this reference",
            ["Page"] = "XAML files that are converted to binary and compiled into the assembly",
            ["PlatformVersionDescription"] = "Platform version description. Used to map between internal OS version and marketing OS version.",
            ["PlatformVersionDescription.OSVersion"] = "Internal OS version.",
            ["PlatformVersionDescription.TargetPlatformIdentifier"] = "Target platform identifier.",
            ["PlatformVersionDescription.TargetPlatformVersion"] = "Target platform version.",
            ["PRIResource"] = "String resources to be indexed in app package's resource index.",
            ["ProjectCapability"] = "Project Capability that may activate design-time components in an IDE.",
            ["ProjectReference"] = "Reference to another project",
            ["ProjectReference.EmbedInteropTypes"] = "Whether the types in this reference need to embedded into the target assembly - interop assemblies only (optional, boolean)",
            ["ProjectReference.ExcludeAssets"] = "Assets to exclude from this reference",
            ["ProjectReference.IncludeAssets"] = "Assets to include from this reference",
            ["ProjectReference.Name"] = "Friendly display name (optional)",
            ["ProjectReference.OutputItemType"] = "Item type to emit target outputs into. Default is blank. If the Reference metadata is set to \"true\" (default) then target outputs will become references for the compiler.",
            ["ProjectReference.PrivateAssets"] = "Assets that are private in this reference",
            ["ProjectReference.Project"] = "Project GUID, in the form {00000000-0000-0000-0000-000000000000}",
            ["ProjectReference.ReferenceOutputAssembly"] = "Boolean specifying whether the outputs of the project referenced should be passed to the compiler. Default is true.",
            ["ProjectReference.SpecificVersion"] = "Whether the exact version of the assembly should be used.",
            ["ProjectReference.Targets"] = "Semicolon separated list of targets in the referenced projects that should be built. Default is the value of $(ProjectReferenceBuildTargets) whose default is blank, indicating the default targets.",
            ["Reference"] = "Reference to an assembly",
            ["Reference.Aliases"] = "Aliases for the reference (optional)",
            ["Reference.EmbedInteropTypes"] = "Whether the types in this reference need to embedded into the target assembly - interop assemblies only (optional, boolean)",
            ["Reference.FusionName"] = "Fusion name of the assembly (optional)",
            ["Reference.HintPath"] = "Relative or absolute path to the assembly (optional)",
            ["Reference.Name"] = "Friendly display name (optional)",
            ["Reference.Private"] = "Whether the reference should be copied to the output folder (optional, boolean)",
            ["Reference.RequiredTargetFramework"] = "The minimum required target framework version in order to use this assembly as a reference",
            ["Reference.SpecificVersion"] = "Whether only the version in the fusion name should be referenced (optional, boolean)",
            ["Resource"] = "File that is compiled into the assembly",
            ["SDKReference"] = "Reference to an extension SDK",
            ["SDKReference.Name"] = "Friendly display name (optional)",
            ["StoreAssociationFile"] = "A file containing app store association data.",
            ["StoreManifestSchema"] = "Store manifest schema file.",
            ["TargetPlatform"] = "Target platform in the form of \"[Identifier], Version=[Version]\", for example, \"Windows, Version=8.0\"",
            ["WebReferences"] = "Name of Web References folder to display in user interface",
            ["WebReferenceUrl"] = "Represents a reference to a web service"
        };
    }
}
