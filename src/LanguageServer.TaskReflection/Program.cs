using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MSBuildProjectTools.LanguageServer.TaskReflection
{
    /// <summary>
    ///     A tool to scan an MSBuild task assembly and output information about the tasks it contains.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The fully-qualified names of supported task parameter types.
        /// </summary>
        static readonly HashSet<string> SupportedTaskParameterTypes = new HashSet<string>
        {
            typeof(string).FullName,
            typeof(bool).FullName,
            typeof(char).FullName,
            typeof(byte).FullName,
            typeof(short).FullName,
            typeof(int).FullName,
            typeof(long).FullName,
            typeof(float).FullName,
            typeof(double).FullName,
            typeof(DateTime).FullName,
            typeof(Guid).FullName,
            "Microsoft.Build.Framework.ITaskItem",
            "Microsoft.Build.Framework.ITaskItem2"
        };

        /// <summary>
        ///     The main program entry-point.
        /// </summary>
        /// <param name="args">
        ///     Command-line arguments.
        /// </param>
        /// <returns>
        ///     0 if successful; otherwise, 1.
        /// </returns>
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                WriteErrorJson("Must specify the task assembly file to examine.");

                return 1;
            }

            try
            {
                // TODO: Consider looking for IntelliDoc XML file to resolve help for tasks and task parameters.

                FileInfo tasksAssemblyFile = new FileInfo(args[0]);
                if (!tasksAssemblyFile.Exists)
                {
                    WriteErrorJson("Cannot find file '{0}'.", tasksAssemblyFile.FullName);

                    return 1;
                }

                string fallbackDirectory = Path.GetDirectoryName(
                    Assembly.GetEntryAssembly().Location
                );
                string baseDirectory = tasksAssemblyFile.DirectoryName;

                DirectoryAssemblyLoadContext loadContext = new DirectoryAssemblyLoadContext(baseDirectory, fallbackDirectory);

                Assembly tasksAssembly = loadContext.LoadFromAssemblyPath(tasksAssemblyFile.FullName);
                if (tasksAssembly == null)
                {
                    WriteErrorJson("Unable to load assembly '{0}'.", tasksAssemblyFile.FullName);

                    return 1;
                }

                Type[] taskTypes;
                try
                {
                    taskTypes = tasksAssembly.GetTypes();
                }
                catch (ReflectionTypeLoadException typeLoadError)
                {
                    taskTypes = typeLoadError.Types;
                }

                taskTypes =
                    taskTypes.Where(
                        type => !type.IsNested && type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(interfaceType => interfaceType.FullName == "Microsoft.Build.Framework.ITask")
                    )
                    .ToArray();

                using (StringWriter output = new StringWriter())
                using (JsonTextWriter jsonWriter = new JsonTextWriter(output))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.WriteStartArray();

                    foreach (Type taskType in taskTypes)
                    {
                        jsonWriter.WriteStartObject();
                        jsonWriter.WritePropertyName("Type");
                        jsonWriter.WriteValue(taskType.FullName);
                        jsonWriter.WritePropertyName("Assembly");
                        jsonWriter.WriteValue(taskType.Assembly.FullName);

                        PropertyInfo[] properties =
                            taskType.GetProperties()
                                .Where(property =>
                                    property.CanRead && property.GetGetMethod().IsPublic
                                    ||
                                    property.CanWrite && property.GetSetMethod().IsPublic
                                )
                                .ToArray();

                        jsonWriter.WritePropertyName("Parameters");
                        jsonWriter.WriteStartArray();
                        foreach (PropertyInfo property in properties)
                        {
                            if (!SupportedTaskParameterTypes.Contains(property.PropertyType.FullName) && !SupportedTaskParameterTypes.Contains(property.PropertyType.FullName + "[]"))
                                continue;

                            jsonWriter.WriteStartObject();

                            jsonWriter.WritePropertyName("Name");
                            jsonWriter.WriteValue(property.Name);

                            jsonWriter.WritePropertyName("Type");
                            jsonWriter.WriteValue(property.PropertyType.FullName);

                            bool isRequired = property.GetCustomAttributes().Any(attribute => attribute.GetType().FullName == "Microsoft.Build.Framework.RequiredAttribute");
                            if (isRequired)
                            {
                                jsonWriter.WritePropertyName("IsRequired");
                                jsonWriter.WriteValue(true);
                            }

                            bool isOutput = property.GetCustomAttributes().Any(attribute => attribute.GetType().FullName == "Microsoft.Build.Framework.OutputAttribute");
                            if (isOutput)
                            {
                                jsonWriter.WritePropertyName("IsOutput");
                                jsonWriter.WriteValue(true);
                            }

                            jsonWriter.WriteEndObject();
                        }
                        jsonWriter.WriteEndArray();

                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteEndArray();
                    jsonWriter.Flush();

                    Console.WriteLine(output);
                }

                return 0;
            }
            catch (Exception unexpectedError)
            {
                System.Diagnostics.Debug.WriteLine(unexpectedError);

                WriteErrorJson(unexpectedError.ToString());

                return 1;
            }
        }

        /// <summary>
        ///     Write an error message in JSON format.
        /// </summary>
        /// <param name="messageOrFormat">
        ///     The error message or message-format specifier.
        /// </param>
        /// <param name="formatArgs">
        ///     Optional message-format arguments.
        /// </param>
        static void WriteErrorJson(string messageOrFormat, params object[] formatArgs)
        {
            string message = formatArgs.Length > 0 ? String.Format(messageOrFormat, formatArgs) : messageOrFormat;

            using (StringWriter output = new StringWriter())
            using (JsonTextWriter jsonWriter = new JsonTextWriter(output))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonWriter.WriteStartObject();
                {
                    jsonWriter.WritePropertyName("Message");
                    jsonWriter.WriteValue(message);
                }
                jsonWriter.WriteEndObject();

                jsonWriter.Flush();
                Console.WriteLine(output);
            }
        }
    }
}
