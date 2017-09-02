using Autofac;
using Serilog;
using System;

namespace MSBuildProjectTools.LanguageServer
{
    using Logging;
    
    /// <summary>
    ///     Registration logic for logging components.
    /// </summary>
    public class LoggingModule
        : Module
    {
        /// <summary>
        ///     Create a new <see cref="LoggingModule"/>.
        /// </summary>
        public LoggingModule()
        {
        }

        /// <summary>
        ///     Configure logging components.
        /// </summary>
        /// <param name="builder">
        ///     The container builder to configure.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            
            builder.Register(CreateLogger)
                .SingleInstance()
                .As<ILogger>();
        }

        /// <summary>
        ///     Create the application logger.
        /// </summary>
        /// <param name="componentContext">
        ///     The current component context.
        /// </param>
        /// <returns>
        ///     The logger.seealso
        /// </returns>
        static ILogger CreateLogger(IComponentContext componentContext)
        {
            if (componentContext == null)
                throw new ArgumentNullException(nameof(componentContext));
            
            Configuration configuration = componentContext.Resolve<Configuration>();
            ConfigureSeq(configuration.Seq);

            Lsp.LanguageServer languageServer = componentContext.Resolve<Lsp.LanguageServer>();

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LanguageServer(languageServer, configuration.LogLevelSwitch);

            if (!String.IsNullOrWhiteSpace(configuration.Seq.Url))
            {
                loggerConfiguration.WriteTo.Seq(configuration.Seq.Url,
                    apiKey: configuration.Seq.ApiKey,
                    controlLevelSwitch: configuration.Seq.LogLevelSwitch
                );
            }

            ILogger logger = loggerConfiguration.CreateLogger();
            Log.Logger = logger;

            return logger;
        }

        /// <summary>
        ///     Configure SEQ logging from environment variables.
        /// </summary>
        /// <param name="configuration">
        ///     The language server's Seq logging configuration.
        /// </param>
        static void ConfigureSeq(SeqLoggingConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            // We have to use environment variables here since at configuration time there's no LSP connection yet.
            configuration.Url = Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_SEQ_URL");
            configuration.ApiKey = Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_SEQ_API_KEY");
        }
    }
}
