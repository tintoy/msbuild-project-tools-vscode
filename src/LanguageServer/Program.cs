using Autofac;
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
                ConfigureLogging();

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
            Log.Verbose("Initialising components...");

            using (IContainer container = BuildContainer())
            {
                var server = container.Resolve<Lsp.LanguageServer>();

                Log.Verbose("Language server started.");

                await server.Initialize();
                await server.WasShutDown;

                Log.Verbose("Language server has shut down.");
            }
        }

        /// <summary>
        ///     Build a container for language server components.
        /// </summary>
        /// <returns>
        ///     The container.
        /// </returns>
        static IContainer BuildContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();
            
            builder.RegisterModule<LoggingModule>();
            builder.RegisterModule<LanguageServerModule>();

            return builder.Build();
        }

        /// <summary>
        ///     Configure logging-to-file (if required) for use before the language server has started.
        /// </summary>
        static void ConfigureLogging()
        {
            string logFilePath = Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_LOG_FILE");
            if (!String.IsNullOrWhiteSpace(logFilePath))
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        path: logFilePath,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}/{Operation}] {Message}{NewLine}{Exception}",
                        flushToDiskInterval: TimeSpan.FromSeconds(1)
                    )
                    .CreateLogger();
            }
        }
    }
}
