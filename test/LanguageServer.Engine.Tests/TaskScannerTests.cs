using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using SemanticModel;
    using Utilities;
    
    /// <summary>
    ///     Tests for <see cref="MSBuildTaskScanner"/>.
    /// </summary>
    public class TaskScannerTests
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildTaskScanner"/> test suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public TaskScannerTests(ITestOutputHelper testOutput)
        {
            if (testOutput == null)
                throw new System.ArgumentNullException(nameof(testOutput));

            TestOutput = testOutput;
        }
        /// <summary>
        ///     Output for the current test.
        /// </summary>
        ITestOutputHelper TestOutput { get; }

        /// <summary>
        ///     Verify that the task scanner can retrieve task metadata from an assembly.
        /// </summary>
        /// <param name="fileName">
        ///     The relative path of the assembly containing the tasks.
        /// </param>
        [InlineData("NuGet.Build.Tasks.dll")]
        [InlineData("Microsoft.Build.Tasks.Core.dll")]
        [InlineData("Sdks/Microsoft.NET.Sdk/tools/netcoreapp1.0/Microsoft.NET.Build.Tasks.dll")]
        [Theory(DisplayName = "TaskScanner can get tasks from framework task assembly ")]
        public async Task Scan_FrameworkTaskAssembly_Success(string fileName)
        {
            string taskAssemblyFile = GetFrameworkTaskAssemblyFile(fileName);
            Assert.True(File.Exists(taskAssemblyFile), "Task assembly exists");

            MSBuildTaskAssemblyMetadata metadata = await MSBuildTaskScanner.GetAssemblyTaskMetadata(taskAssemblyFile);
            Assert.NotNull(metadata);

            Assert.NotEqual(0, metadata.Tasks.Count);

            foreach (MSBuildTaskMetadata taskMetadata in metadata.Tasks.OrderBy(task => task.TypeName))
                TestOutput.WriteLine("Found task '{0}'.", taskMetadata.TypeName);
        }

        /// <summary>
        ///     Retrieve the full path of a task assembly supplied as part of the current framework.
        /// </summary>
        /// <param name="assemblyFileName">
        ///     The relative filename of the task assembly.
        /// </param>
        /// <returns>
        ///     The full path of the task assembly.
        /// </returns>
        static string GetFrameworkTaskAssemblyFile(string assemblyFileName)
        {
            if (string.IsNullOrWhiteSpace(assemblyFileName))
                throw new System.ArgumentException($"Argument cannot be null, empty, or entirely composed of whitespace: {nameof(assemblyFileName)}.", nameof(assemblyFileName));

            var runtimeInfo = DotNetRuntimeInfo.GetCurrent();

            return Path.Combine(runtimeInfo.BaseDirectory,
                assemblyFileName.Replace('/', Path.DirectorySeparatorChar)
            );
        }

        /// <summary>
        ///     Type initialiser for <see cref="TaskScannerTests"/>.
        /// </summary>
        /// <remarks>
        ///     TODO: Use a collection / fixture.
        /// </remarks>
        static TaskScannerTests()
        {
            // Ensure that the scanner can find the task reflector.
            MSBuildTaskScanner.TaskReflectorAssemblyFile = new FileInfo(
                Path.Combine(
                    Path.GetDirectoryName(typeof(TaskScannerTests).Assembly.Location),
                    "..", "..", "..", "..", "..", "src", "LanguageServer.TaskReflection", "bin", "debug", "netcoreapp2.0",
                    "MSBuildProjectTools.LanguageServer.TaskReflection.dll"
                )
            );
        }
    }
}
