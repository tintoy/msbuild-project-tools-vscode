using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using SemanticModel;
    using Utilities;

    /// <summary>
    ///     Tests for MSBuild integration.
    /// </summary>
    public class MSBuildTests
    {
        /// <summary>
        ///     The directory for test files.
        /// </summary>
        static readonly DirectoryInfo TestDirectory = new DirectoryInfo(Path.GetDirectoryName(
            new Uri(typeof(XmlLocatorTests).Assembly.CodeBase).LocalPath
        ));

        /// <summary>
        ///     Create a new MSBuild integration test suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public MSBuildTests(ITestOutputHelper testOutput)
        {
            if (testOutput == null)
                throw new ArgumentNullException(nameof(testOutput));

            TestOutput = testOutput;
        }

        /// <summary>
        ///     Output for the current test.
        /// </summary>
        public ITestOutputHelper TestOutput { get; }

        /// <summary>
        ///     Dump all UsingTask elements in an MSBuild project.
        /// </summary>
        [InlineData("Project1")]
        [Theory(DisplayName = "Dump all UsingTask elements in an MSBuild project ")]
        public void DumpUsingTasks(string projectName)
        {
            Project project = LoadTestProject(projectName + ".csproj");
            using (project.ProjectCollection)
            {
                foreach (ProjectUsingTaskElement usingTaskElement in project.GetAllUsingTasks())
                {
                    TestOutput.WriteLine("UsingTask '{0}' from '{1}':",
                        usingTaskElement.TaskName,
                        usingTaskElement.ContainingProject.FullPath
                    );
                    TestOutput.WriteLine("\tAssemblyFile: '{0}'",
                        project.ExpandString(usingTaskElement.AssemblyFile)
                    );
                    TestOutput.WriteLine("\tAssemblyName: '{0}'",
                        project.ExpandString(usingTaskElement.AssemblyName)
                    );
                    TestOutput.WriteLine("\tParameterGroup.Count: '{0}'",
                        usingTaskElement.ParameterGroup?.Count ?? 0
                    );
                    TestOutput.WriteLine("\tRuntime: '{0}'",
                        project.ExpandString(usingTaskElement.Runtime)
                    );
                    TestOutput.WriteLine("\tTaskFactory: '{0}'",
                        project.ExpandString(usingTaskElement.TaskFactory)
                    );
                }
            }
        }

        /// <summary>
        ///     Load a test project.
        /// </summary>
        /// <param name="relativePathSegments">
        ///     The project file's relative path segments.
        /// </param>
        /// <returns>
        ///     The project.
        /// </returns>
        static Project LoadTestProject(params string[] relativePathSegments)
        {
            if (relativePathSegments == null)
                throw new ArgumentNullException(nameof(relativePathSegments));

            return MSBuildHelper.CreateProjectCollection(TestDirectory.FullName).LoadProject(
                Path.Combine(TestDirectory.FullName,
                    "TestProjects",
                    Path.Combine(relativePathSegments)
                )
            );
        }
    }
}
