using Lsp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer
{
    using Documents;
    using Handlers;
    using Logging;

    /// <summary>
    ///     The MSBuild language server.
    /// </summary>
    static class Program
    {
        /// <summary>
        ///     The main program entry-point.
        /// </summary>
        static void Main()
        {
            SynchronizationContext.SetSynchronizationContext(
                new SynchronizationContext()
            );

            try
            {
                AsyncMain().Wait();
            }
            catch (AggregateException aggregateError)
            {
                foreach (Exception unexpectedError in aggregateError.Flatten().InnerExceptions)
                {
                    Console.WriteLine(unexpectedError);
                }
            }
            catch (Exception unexpectedError)
            {
                Console.WriteLine(unexpectedError);
            }
        }

        /// <summary>
        ///     The asynchronous program entry-point.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task"/> representing program execution.
        /// </returns>
        static async Task AsyncMain()
        {
            var server = new Lsp.LanguageServer(
                input: Console.OpenStandardInput(),
                output: Console.OpenStandardOutput()
            );

            Configuration configuration = new Configuration();
            ConfigureLogging(server, configuration);

            Workspace workspace = new Workspace(server, configuration, Log.Logger);
            server.AddHandler(
                new ConfigurationHandler(configuration)
            );
            server.AddHandler(
                new DocumentSyncHandler(server, workspace, Log.Logger)
            );
            server.AddHandler(
                new HoverHandler(server, workspace, Log.Logger)
            );
            server.AddHandler(
                new DocumentSymbolHandler(server, workspace, Log.Logger)
            );
            server.AddHandler(
                new CompletionHandler(server, workspace, Log.Logger)
            );
            server.AddHandler(
                new DefinitionHandler(server, workspace, Log.Logger)
            );

            await server.Initialize();
            await server.WasShutDown;
        }

        /// <summary>
        ///     Configure Serilog to write log events to the language server.
        /// </summary>
        /// <param name="languageServer">
        ///     The language server.
        /// </param>
        /// <param name="configuration">
        ///     The language server configuration.
        /// </param>
        static void ConfigureLogging(Lsp.LanguageServer languageServer, Configuration configuration)
        {
            if (languageServer == null)
                throw new ArgumentNullException(nameof(languageServer));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.LanguageServer(languageServer, configuration);

            string seqServerUrl = Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_SEQ_URL");
            if (!String.IsNullOrWhiteSpace(seqServerUrl))
            {
                loggerConfiguration.WriteTo.Seq(seqServerUrl,
                    apiKey: Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_SEQ_API_KEY")
                );

                // TODO: Use LoggingLevelSwitch.
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
    }
}
