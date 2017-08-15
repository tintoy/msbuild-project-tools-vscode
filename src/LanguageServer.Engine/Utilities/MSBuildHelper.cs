using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Helper methods for working with MSBuild projects.
    /// </summary>
    public static class MSBuildHelper
    {
        /// <summary>
        ///     Create an MSBuild project collection.
        /// </summary>
        /// <param name="solutionDirectory">
        ///     The base (i.e. solution) directory.
        /// </param>
        /// <returns>
        ///     The project collection.
        /// </returns>
        public static ProjectCollection CreateProjectCollection(string solutionDirectory)
        {
            return CreateProjectCollection(solutionDirectory,
                DotNetRuntimeInfo.GetCurrent()
            );
        }

        /// <summary>
        ///     Create an MSBuild project collection.
        /// </summary>
        /// <param name="solutionDirectory">
        ///     The base (i.e. solution) directory.
        /// </param>
        /// <param name="runtimeInfo">
        ///     Information about the current .NET Core runtime.
        /// </param>
        /// <returns>
        ///     The project collection.
        /// </returns>
        public static ProjectCollection CreateProjectCollection(string solutionDirectory, DotNetRuntimeInfo runtimeInfo)
        {
            if (String.IsNullOrWhiteSpace(solutionDirectory))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'baseDir'.", nameof(solutionDirectory));

            if (runtimeInfo == null)
                throw new ArgumentNullException(nameof(runtimeInfo));
            
            if (String.IsNullOrWhiteSpace(runtimeInfo.BaseDirectory))
                throw new InvalidOperationException("Cannot determine base directory for .NET Core.");

            Dictionary<string, string> globalProperties = CreateGlobalMSBuildProperties(runtimeInfo, solutionDirectory);
            EnsureMSBuildEnvironment(globalProperties);

            ProjectCollection projectCollection = new ProjectCollection(globalProperties);

            // Override toolset paths (for some reason these point to the main directory where the dotnet executable lives).
            Toolset toolset = projectCollection.GetToolset("15.0");
            toolset = new Toolset(
                toolsVersion: "15.0",
                toolsPath: globalProperties["MSBuildExtensionsPath"],
                projectCollection: projectCollection,
                msbuildOverrideTasksPath: ""
            );
            projectCollection.AddToolset(toolset);

            return projectCollection;
        }
        
        /// <summary>
        ///     Create global properties for MSBuild.
        /// </summary>
        /// <param name="runtimeInfo">
        ///     Information about the current .NET Core runtime.
        /// </param>
        /// <param name="solutionDirectory">
        ///     The base (i.e. solution) directory.
        /// </param>
        /// <returns>
        ///     A dictionary containing the global properties.
        /// </returns>
        public static Dictionary<string, string> CreateGlobalMSBuildProperties(DotNetRuntimeInfo runtimeInfo, string solutionDirectory)
        {
            if (runtimeInfo == null)
                throw new ArgumentNullException(nameof(runtimeInfo));

            if (String.IsNullOrWhiteSpace(solutionDirectory))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'solutionDirectory'.", nameof(solutionDirectory));
            
            if (solutionDirectory.Length > 0 && solutionDirectory[solutionDirectory.Length - 1] != Path.DirectorySeparatorChar)
                solutionDirectory += Path.DirectorySeparatorChar;
            
            return new Dictionary<string, string>
            {
                [WellKnownProperties.DesignTimeBuild] = "true",
                [WellKnownProperties.BuildProjectReferences] = "false",
                [WellKnownProperties.ResolveReferenceDependencies] = "true",
                [WellKnownProperties.SolutionDir] = solutionDirectory,
                [WellKnownProperties.MSBuildExtensionsPath] = runtimeInfo.BaseDirectory,
                [WellKnownProperties.MSBuildSDKsPath] = Path.Combine(runtimeInfo.BaseDirectory, "Sdks"),
                [WellKnownProperties.RoslynTargetsPath] = Path.Combine(runtimeInfo.BaseDirectory, "Roslyn")
            };
        }

        /// <summary>
        ///     Ensure that environment variables are populated using the specified MSBuild global properties.
        /// </summary>
        /// <param name="globalMSBuildProperties">
        ///     The MSBuild global properties
        /// </param>
        public static void EnsureMSBuildEnvironment(Dictionary<string, string> globalMSBuildProperties)
        {
            if (globalMSBuildProperties == null)
                throw new ArgumentNullException(nameof(globalMSBuildProperties));

            // Kinda sucks that the simplest way to get MSBuild to resolve SDKs correctly is using environment variables, but there you go.
            Environment.SetEnvironmentVariable(
                WellKnownProperties.MSBuildExtensionsPath,
                globalMSBuildProperties[WellKnownProperties.MSBuildExtensionsPath]
            );
            Environment.SetEnvironmentVariable(
                WellKnownProperties.MSBuildSDKsPath,
                globalMSBuildProperties[WellKnownProperties.MSBuildSDKsPath]
            );
        }

        /// <summary>
        ///     The names of well-known MSBuild properties.
        /// </summary>
        public static class WellKnownProperties
        {
            /// <summary>
            ///     The "MSBuildExtensionsPath" property.
            /// </summary>
            public static readonly string MSBuildExtensionsPath = "MSBuildExtensionsPath";

            /// <summary>
            ///     The "MSBuildSDKsPath" property.
            /// </summary>
            public static readonly string MSBuildSDKsPath = "MSBuildSDKsPath";

            /// <summary>
            ///     The "SolutionDir" property.
            /// </summary>
            public static readonly string SolutionDir = "SolutionDir";

            /// <summary>
            ///     The "_ResolveReferenceDependencies" property.
            /// </summary>
            public static readonly string ResolveReferenceDependencies = "_ResolveReferenceDependencies";

            /// <summary>
            ///     The "DesignTimeBuild" property.
            /// </summary>
            public static readonly string DesignTimeBuild = "DesignTimeBuild";

            /// <summary>
            ///     The "BuildProjectReferences" property.
            /// </summary>
            public static readonly string BuildProjectReferences = "BuildProjectReferences";

            /// <summary>
            ///     The "RoslynTargetsPath" property.
            /// </summary>
            public static readonly string RoslynTargetsPath = "RoslynTargetsPath";
        }
    }
}
