using JsonRpc;
using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Models;
using Lsp.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;
    
namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using CustomProtocol;
    using Handlers;

    /// <summary>
    ///     Language Server message handler that tracks configuration.
    /// </summary>
    public sealed class ConfigurationHandler
        : IDidChangeConfigurationSettingsHandler
    {
        /// <summary>
        ///     The JSON serialiser used to read settings from LSP notifications.
        /// </summary>
        /// <returns></returns>
        readonly JsonSerializer _settingsSerializer = new JsonSerializer();

        /// <summary>
        ///     Create a new <see cref="ConfigurationHandler"/>.
        /// </summary>
        /// <param name="configuration">
        ///     The language server configuration.
        /// </param>
        public ConfigurationHandler(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Configuration = configuration;
        }

        /// <summary>
        ///     Raised when configuration has changed.
        /// </summary>
        public event EventHandler<EventArgs> ConfigurationChanged;

        /// <summary>
        ///     The language server configuration.
        /// </summary>
        public Configuration Configuration { get; }

        /// <summary>
        ///     The server's configuration capabilities.
        /// </summary>
        DidChangeConfigurationCapability ConfigurationCapabilities { get; set; }

        /// <summary>
        ///     Called when configuration has changed.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        Task OnDidChangeConfiguration(DidChangeConfigurationObjectParams parameters)
        {
            Configuration.UpdateFrom(parameters);

            if (ConfigurationChanged != null)
                ConfigurationChanged(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called to inform the handler of the language server's configuration capabilities.
        /// </summary>
        /// <param name="capabilities">
        ///     A <see cref="SynchronizationCapability"/> data structure representing the capabilities.
        /// </param>
        void ICapability<DidChangeConfigurationCapability>.SetCapability(DidChangeConfigurationCapability capabilities)
        {
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            ConfigurationCapabilities = capabilities;
        }

        /// <summary>
        ///     Handle a change in configuration.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        async Task INotificationHandler<DidChangeConfigurationObjectParams>.Handle(DidChangeConfigurationObjectParams parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            
            try
            {
                await OnDidChangeConfiguration(parameters);
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unhandled exception in {Method:l}.", "OnDidChangeConfiguration");
            }
        }

        /// <summary>
        ///     Unused.
        /// </summary>
        /// <returns>
        ///     <c>null</c>
        /// </returns>
        object IRegistration<object>.GetRegistrationOptions()
        {
            return null;
        }
    }
}
