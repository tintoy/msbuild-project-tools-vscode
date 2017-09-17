using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer.Help
{
    /// <summary>
    ///     Help information for an MSBuild task.
    /// </summary>
    public class TaskHelp
    {
        /// <summary>
        ///     A description of the task.
        /// </summary>
        [JsonProperty("summary")]
        public string Description { get; set; }

        /// <summary>
        ///     The task's parameters.
        /// </summary>
        [JsonProperty("parameters", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public SortedDictionary<string, TaskParameterHelp> Parameters { get; set; }

        /// <summary>
        ///     Load task help from JSON.
        /// </summary>
        /// <param name="json">
        ///     A <see cref="JsonReader"/> representing the JSON.
        /// </param>
        /// <returns>
        ///     A sorted dictionary of task help, keyed by task name.
        /// </returns>
        public static SortedDictionary<string, TaskHelp> FromJson(JsonReader json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            
            return new JsonSerializer().Deserialize<SortedDictionary<string, TaskHelp>>(json);
        }
    }

    /// <summary>
    ///     Help information for an MSBuild task parameter.
    /// </summary>
    public class TaskParameterHelp
    {
        /// <summary>
        ///     A description of the task parameter.
        /// </summary>
        [JsonProperty("summary")]
        public string Description { get; set; }

        /// <summary>
        ///     A description of the task parameter data-type.
        /// </summary>
        [JsonProperty("type")]
        public string TypeDescription { get; set; }
    }
}
