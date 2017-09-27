using OmniSharp.Extensions.LanguageServer;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MSBuildProjectTools.LanguageServer.CustomProtocol
{
    /// <summary>
    ///     Custom Language Server Protocol extensions.
    /// </summary>
    public static class ProtocolExtensions
    {
        /// <summary>
        ///     Notify the language client that the language service is busy.
        /// </summary>
        /// <param name="router">
        ///     The language server used to route messages to the client.
        /// </param>
        /// <param name="message">
        ///     A message describing why the language service is busy.
        /// </param>
        public static void NotifyBusy(this ILanguageServer router, string message)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));

            if (String.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'message'.", nameof(message));

            router.SendNotification("msbuild/busy", new BusyNotificationParams
            {
                IsBusy = true,
                Message = message
            });
        }

        /// <summary>
        ///     Notify the language client that the language service is no longer busy.
        /// </summary>
        /// <param name="router">
        ///     The language server used to route messages to the client.
        /// </param>
        /// <param name="message">
        ///     An optional message indicating the operation that was completed.
        /// </param>
        public static void ClearBusy(this ILanguageServer router, string message = null)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));

            router.SendNotification("msbuild/busy", new BusyNotificationParams
            {
                IsBusy = false,
                Message = message
            });
        }

        /// <summary>
        ///     Update the configuration from the specified notification parameters.
        /// </summary>
        /// <param name="configuration">
        ///     The <see cref="Configuration"/> to update.
        /// </param>
        /// <param name="parameters">
        ///     The DidChangeConfiguration notification parameters.
        /// </param>
        public static void UpdateFrom(this Configuration configuration, DidChangeConfigurationObjectParams parameters)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            JObject settings = parameters.Settings?.SelectToken("msbuildProjectTools.language") as JObject;
            if (settings == null)
                return;

            // Temporary workaround - JsonSerializer.Populate reuses existing HashSet.
            configuration.CompletionsFromProject.Clear();
            configuration.EnableExperimentalFeatures.Clear();

            using (JsonReader reader = settings.CreateReader())
            {
                new JsonSerializer().Populate(reader, configuration);
            }
        }
    }
}
