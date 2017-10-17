using Autofac;
using OmniSharp.Extensions.LanguageServer;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MSLogging = Microsoft.Extensions.Logging;

namespace MSBuildProjectTools.LanguageServer
{
    using CompletionProviders;
    using CustomProtocol;
    using Diagnostics;
    using Handlers;
    using LanguageServer = OmniSharp.Extensions.LanguageServer.LanguageServer;

    /// <summary>
    ///     Registration logic for language server components.
    /// </summary>
    public class LanguageServerModule
        : Module
    {
        /// <summary>
        ///     Create a new <see cref="LanguageServerModule"/>.
        /// </summary>
        public LanguageServerModule()
        {
        }

        /// <summary>
        ///     Configure language server components.
        /// </summary>
        /// <param name="builder">
        ///     The container builder to configure.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterInstance(Configuration).AsSelf();
            
            builder
                .Register(componentContext => new LanguageServer(
                    input: Console.OpenStandardInput(),
                    output: Console.OpenStandardOutput(),
                    loggerFactory: componentContext.Resolve<MSLogging.ILoggerFactory>()
                ))
                .AsSelf()
                .As<ILanguageServer>()
                .SingleInstance()
                .OnActivated(activated =>
                {
                    LanguageServer languageServer = activated.Instance;
                    
                    // Register configuration handler (which is not a Handler).
                    var configurationHandler = activated.Context.Resolve<ConfigurationHandler>();
                    languageServer.AddHandler(configurationHandler);

                    languageServer.OnInitialize(initializationParameters =>
                    {
                        configurationHandler.Configuration.UpdateFrom(initializationParameters);

                        return Task.CompletedTask;
                    });

                    // Register all other handlers.
                    var handlers = activated.Context.Resolve<IEnumerable<Handler>>();
                    foreach (Handler handler in handlers)
                        languageServer.AddHandler(handler);
                });

            builder.RegisterType<LspDiagnosticsPublisher>()
                .As<IPublishDiagnostics>()
                .InstancePerDependency();

            builder.RegisterType<ConfigurationHandler>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<Documents.Workspace>()
                .AsSelf()
                .SingleInstance()
                .OnActivated(activated =>
                {
                    Documents.Workspace workspace = activated.Instance;
                    workspace.RestoreTaskMetadataCache();
                });

            builder
                .RegisterTypes(
                    typeof(ConfigurationHandler),
                    typeof(DocumentSyncHandler),
                    typeof(DocumentSymbolHandler),
                    typeof(DefinitionHandler),
                    typeof(HoverHandler)
                )
                .AsSelf()
                .As<Handler>()
                .SingleInstance();

            builder.RegisterType<CompletionHandler>()
                .AsSelf().As<Handler>()
                .SingleInstance()
                .OnActivated(activated =>
                {
                    CompletionHandler completionHandler = activated.Instance;

                    completionHandler.Providers.AddRange(
                        activated.Context.Resolve<IEnumerable<ICompletionProvider>>()
                    );
                });

            Type completionProviderType = typeof(CompletionProvider);
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(
                    type => type.IsSubclassOf(completionProviderType) && !type.IsAbstract
                )
                .AsSelf()
                .As<CompletionProvider>()
                .As<ICompletionProvider>()
                .SingleInstance();
        }

        /// <summary>
        ///     The language server configuration.
        /// </summary>
        public Configuration Configuration { get; } = new Configuration();
    }
}
