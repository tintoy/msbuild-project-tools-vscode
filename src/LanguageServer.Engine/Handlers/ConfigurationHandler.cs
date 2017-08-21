using JsonRpc;
using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Models;
using Lsp.Protocol;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;
    
namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using Handlers;

    /// <summary>
    ///     Language Server message handler that tracks configuration.
    /// </summary>
    public sealed class ConfigurationHandler
        : Handler, IDidChangeConfigurationHandler
    {
        /// <summary>
        ///     Create a new <see cref="LoggingConfigurationHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server that hosts the handler.
        /// </param>
        public ConfigurationHandler(ILanguageServer server)
            : base(server, Serilog.Log.Logger)
        {
        }

        /// <summary>
        ///     The currently-configured minimum log level.
        /// </summary>
        public LogEventLevel LogLevel { get; private set; } = LogEventLevel.Information;

        /// <summary>
        ///     Disable tooltips when hovering on XML in MSBuild project files?
        /// </summary>
        public bool DisableHover { get; private set; } = false;

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
        Task OnDidChangeConfiguration(Lsp.Models.DidChangeConfigurationParams parameters)
        {
            if (parameters.Settings.TryGetValue("logLevel", out Lsp.Models.BooleanNumberString logLevelValue) && logLevelValue.IsString)
            {
                LogEventLevel configuredLogLevel;
                if (!Enum.TryParse(logLevelValue.String, true, out configuredLogLevel))
                    configuredLogLevel = LogEventLevel.Information;
                
                LogLevel = configuredLogLevel;
            }
            
            if (parameters.Settings.TryGetValue("disableHover", out Lsp.Models.BooleanNumberString disableHover) && disableHover.IsBool)
                DisableHover = disableHover.Bool;

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
        async Task INotificationHandler<DidChangeConfigurationParams>.Handle(DidChangeConfigurationParams parameters)
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
