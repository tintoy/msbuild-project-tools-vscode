using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer.Help
{
    /// <summary>
    ///     Help information for an MSBuild item.
    /// </summary>
    public class ItemHelp
    {
        /// <summary>
        ///     A description of the item.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        ///     Descriptions for the item's metadata.
        /// </summary>
        [JsonProperty("metadata", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public SortedDictionary<string, string> Metadata { get; } = new SortedDictionary<string, string>();

        /// <summary>
        ///     Load item help from JSON.
        /// </summary>
        /// <param name="json">
        ///     A <see cref="JsonReader"/> representing the JSON.
        /// </param>
        /// <returns>
        ///     A sorted dictionary of item help, keyed by item name.
        /// </returns>
        public static SortedDictionary<string, ItemHelp> FromJson(JsonReader json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            
            return new JsonSerializer().Deserialize<SortedDictionary<string, ItemHelp>>(json);
        }
    }
}
