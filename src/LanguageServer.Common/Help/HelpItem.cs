using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer.Help
{
    /// <summary>
    ///     Basic information common to all help items.
    /// </summary>
    public class HelpItem
    {
        /// <summary>
        ///     The item description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        ///     Load help items from JSON.
        /// </summary>
        /// <param name="json">
        ///     A <see cref="JsonReader"/> representing the JSON ("ItemName": { "description": "ItemDescription" }).
        /// </param>
        /// <returns>
        ///     A sorted dictionary of help items, keyed by item name.
        /// </returns>
        public static SortedDictionary<string, HelpItem> FromJson(JsonReader json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            
            return new JsonSerializer().Deserialize<SortedDictionary<string, HelpItem>>(json);
        }
    }
}
