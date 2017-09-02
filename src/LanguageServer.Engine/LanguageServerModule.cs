using Autofac;
using Serilog;
using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer
{
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

            builder.RegisterInstance(Configuration);
            
            builder
                .Register(_ => new Lsp.LanguageServer(
                    input: Console.OpenStandardInput(),
                    output: Console.OpenStandardOutput()
                ))
                .AsSelf().As<Lsp.ILanguageServer>()
                .SingleInstance()
                .OnActivated(activated =>
                {
                    Lsp.LanguageServer languageServer = activated.Instance;
                    
                    // Register configuration handler (which is not a Handlers.Handler).
                    var configurationHandler = activated.Context.Resolve<Handlers.ConfigurationHandler>();
                    languageServer.AddHandler(configurationHandler);

                    // Register all other handlers.
                    var handlers = activated.Context.Resolve<IEnumerable<Handlers.Handler>>();
                    foreach (Handlers.Handler handler in handlers)
                        languageServer.AddHandler(handler);
                });

            builder.RegisterType<Handlers.ConfigurationHandler>().AsSelf()
                .SingleInstance();

            builder.RegisterType<Documents.Workspace>().AsSelf()
                .SingleInstance();

            Type handlerType = typeof(Handlers.Handler);
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(
                    type => !type.IsAbstract && type.IsSubclassOf(handlerType)
                )
                .AsSelf().As<Handlers.Handler>()
                .SingleInstance();

            Type completionProviderType = typeof(CompletionProviders.CompletionProvider);
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(
                    type => !type.IsAbstract && type.IsSubclassOf(completionProviderType)
                )
                .SingleInstance();
        }

        /// <summary>
        ///     The language server configuration.
        /// </summary>
        public Configuration Configuration { get; } = new Configuration();
    }
}
