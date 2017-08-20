using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    using Microsoft.Build.Construction;
    using Utilities;

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
                DotNetRuntimeInfo.GetCurrent(solutionDirectory)
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
                [MSBuildPropertyNames.DesignTimeBuild] = "true",
                [MSBuildPropertyNames.BuildProjectReferences] = "false",
                [MSBuildPropertyNames.ResolveReferenceDependencies] = "true",
                [MSBuildPropertyNames.SolutionDir] = solutionDirectory,
                [MSBuildPropertyNames.MSBuildExtensionsPath] = runtimeInfo.BaseDirectory,
                [MSBuildPropertyNames.MSBuildSDKsPath] = Path.Combine(runtimeInfo.BaseDirectory, "Sdks"),
                [MSBuildPropertyNames.RoslynTargetsPath] = Path.Combine(runtimeInfo.BaseDirectory, "Roslyn")
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
                MSBuildPropertyNames.MSBuildExtensionsPath,
                globalMSBuildProperties[MSBuildPropertyNames.MSBuildExtensionsPath]
            );
            Environment.SetEnvironmentVariable(
                MSBuildPropertyNames.MSBuildSDKsPath,
                globalMSBuildProperties[MSBuildPropertyNames.MSBuildSDKsPath]
            );
        }

        /// <summary>
        ///     Get the <see cref="Range"/> represented by the <see cref="InvalidProjectFileException"/>.
        /// </summary>
        /// <param name="invalidProjectFileException">
        ///     The <see cref="InvalidProjectFileException"/>.
        /// </param>
        /// <returns>
        ///     The <see cref="Range"/>.
        /// </returns>
        public static Range GetRange(this InvalidProjectFileException invalidProjectFileException)
        {
            if (invalidProjectFileException == null)
                throw new ArgumentNullException(nameof(invalidProjectFileException));

            Position startPosition = new Position(
                invalidProjectFileException.LineNumber,
                invalidProjectFileException.ColumnNumber
            );

            Position endPosition = new Position(
                invalidProjectFileException.EndLineNumber,
                invalidProjectFileException.EndColumnNumber
            );
            if (endPosition == Position.Zero)
                endPosition = startPosition;
            
            return new Range(startPosition, endPosition);
        }

        /// <summary>
        ///     Get the condition (if any) declared on the element or one of its ancestors.
        /// </summary>
        /// <param name="projectElement">
        ///     The element.
        /// </param>
        /// <returns>
        ///     The condition, or an empty string if no condition is present on the element or one of its ancestors.
        /// </returns>
        public static string FindCondition(this ProjectElement projectElement)
        {
            if (projectElement == null)
                throw new ArgumentNullException(nameof(projectElement));
            
            ProjectElement currentElement = projectElement;
            while (currentElement != null)
            {
                if (!String.IsNullOrWhiteSpace(currentElement.Condition))
                    return currentElement.Condition;

                currentElement = currentElement.Parent;
            }

            return String.Empty;
        }
    }
}
