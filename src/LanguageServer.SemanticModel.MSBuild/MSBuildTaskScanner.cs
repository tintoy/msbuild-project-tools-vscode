using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    // TODO: Asynchronously scan the project for UsingTask references, and then scan and cache the tasks.

    /// <summary>
    ///     Scans for MSBuild task assemblies for metadata.
    /// </summary>
    public static class MSBuildTaskScanner
    {
        /// <summary>
        ///     The assembly file containing the task-reflector tool.
        /// </summary>
        internal static FileInfo TaskReflectorAssemblyFile = new FileInfo(
            Path.GetFullPath(
                Path.Combine(
                    Assembly.GetEntryAssembly().Location, "..", "task-reflection",
                    "MSBuildProjectTools.LanguageServer.TaskReflection.dll"
                )
            )
        );

        /// <summary>
        ///     Get task metadata for the specified assembly.
        /// </summary>
        /// <param name="taskAssemblyPath">
        ///     The full path to the assembly containing the task.
        /// </param>
        /// <returns>
        ///     A list of <see cref="MSBuildTaskMetadata"/> representing the tasks.
        /// </returns>
        public static List<MSBuildTaskMetadata> GetAssemblyTaskMetadata(string taskAssemblyPath)
        {
            if (string.IsNullOrWhiteSpace(taskAssemblyPath))
                throw new ArgumentException($"Argument cannot be null, empty, or entirely composed of whitespace: {nameof(taskAssemblyPath)}.", nameof(taskAssemblyPath));

            ProcessStartInfo scannerStartInfo = new ProcessStartInfo("dotnet")
            {
                Arguments = $"\"{TaskReflectorAssemblyFile.FullName}\" \"{taskAssemblyPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            using (Process scannerProcess = Process.Start(scannerStartInfo))
            {
                scannerProcess.WaitForExit();

                using (StreamReader scannerOutput = scannerProcess.StandardOutput)
                using (JsonTextReader scannerJson = new JsonTextReader(scannerOutput))
                {
                    if (scannerProcess.ExitCode == 0)
                        return new JsonSerializer().Deserialize<List<MSBuildTaskMetadata>>(scannerJson);

                    JObject errorJson = JObject.Load(scannerJson);
                    string message = errorJson.Value<string>("Message");
                    if (String.IsNullOrWhiteSpace(message))
                        message = $"An unexpected error occurred while scanning assembly '{taskAssemblyPath}' for tasks.";

                    // TODO: Custom exception type.

                    throw new Exception(message);
                }
            }
        }
    }

    /// <summary>
    ///     Metadata for an assembly containing MSBuild tasks.
    /// </summary>
    public class MSBuildTaskAssemblyMetadata
    {
        /// <summary>
        ///     The assembly's full name.
        /// </summary>
        [JsonProperty("assemblyName")]
        public DateTime AssemblyName { get; set; }

        /// <summary>
        ///     The full path to the assembly file.
        /// </summary>
        [JsonProperty("assemblyPath")]
        public DateTime AssemblyPath { get; set; }

        /// <summary>
        ///     The assembly file's timestamp.
        /// </summary>
        [JsonProperty("timestampUtc")]
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        ///     Tasks defined in the assembly.
        /// </summary>
        [JsonProperty("tasks", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public List<MSBuildTaskMetadata> Tasks { get; } = new List<MSBuildTaskMetadata>();
    }

    /// <summary>
    ///     Metadata for an MSBuild task.
    /// </summary>
    public class MSBuildTaskMetadata
    {
        /// <summary>
        ///     The full name of the type that implements the task.
        /// </summary>
        [JsonProperty("typeName")]
        public string TypeName { get; set; }

        /// <summary>
        ///     The full name of the assembly that contains the task.
        /// </summary>
        [JsonProperty("assemblyName")]
        public string AssemblyName { get; set; }

        /// <summary>
        ///     The task parameters (if any).
        /// </summary>
        [JsonProperty("parameters", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public List<MSBuildTaskParameterMetadata> Parameters { get; } = new List<MSBuildTaskParameterMetadata>();
    }

    /// <summary>
    ///     Metadata for a parameter of an MSBuild task.
    /// </summary>
    public class MSBuildTaskParameterMetadata
    {
        /// <summary>
        ///     The parameter name.
        /// </summary>
        [JsonProperty("parameterName")]
        public string Name { get; set; }

        /// <summary>
        ///     The full name of the parameter's data type.
        /// </summary>
        [JsonProperty("parameterType")]
        public string TypeName { get; set; }

        /// <summary>
        ///     Is the parameter mandatory?
        /// </summary>
        [JsonProperty("required")]
        public bool IsRequired { get; set; }

        /// <summary>
        ///     Is the parameter an output parameter?
        /// </summary>
        [JsonProperty("output")]
        public bool IsOutput { get; set; }
    }
}
