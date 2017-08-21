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

            ConfigurationHandler configuration = new ConfigurationHandler();
            server.AddHandler(configuration);

            ConfigureLogging(server, configuration.Configuration);

            server.AddHandler(
                new ProjectDocumentHandler(server, configuration.Configuration, Log.Logger)
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
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.LanguageServer(languageServer, configuration)
                .CreateLogger();
        }
    }
}
