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

            ConfigureLogging(server);

            server.AddHandler(
                new ProjectDocumentHandler(server, Log.Logger)
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
        static void ConfigureLogging(Lsp.LanguageServer languageServer)
        {
            if (languageServer == null)
                throw new ArgumentNullException(nameof(languageServer));
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.LanguageServer(languageServer)
                .CreateLogger();
        }
    }
}
