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
        ///     The language server configuration.
        /// </summary>
        public Configuration Configuration { get; }

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
            Lsp.LanguageServer languageServer = componentContext.Resolve<Lsp.LanguageServer>();

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.LanguageServer(languageServer, configuration.LogLevelSwitch);

            string seqServerUrl = Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_SEQ_URL");
            if (!String.IsNullOrWhiteSpace(seqServerUrl))
            {
                loggerConfiguration.WriteTo.Seq(seqServerUrl,
                    apiKey: Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_SEQ_API_KEY"),
                    controlLevelSwitch: configuration.SeqLogLevelSwitch
                );
            }

            ILogger logger = loggerConfiguration.CreateLogger();
            Log.Logger = logger;

            return logger;
        }
    }
}
