using Lsp;
using Lsp.Models;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            SynchronizationContext.SetSynchronizationContext(
                new SynchronizationContext()
            );

            ConfigureLogging();

            try
            {
                AsyncMain(args).Wait();
            }
            catch (AggregateException aggregateError)
            {
                foreach (Exception unexpectedError in aggregateError.InnerExceptions)
                {
                    Log.Error(unexpectedError, "Unexpected error: {Message}", unexpectedError.Message);
                }
            }
            catch (Exception unexpectedError)
            {
                Log.Error(unexpectedError, "Unexpected error: {Message}", unexpectedError.Message);
            }
        }

        static async Task AsyncMain(string[] args)
        {
            var server = new Lsp.LanguageServer(Console.OpenStandardInput(), Console.OpenStandardOutput());

            await server.Initialize();
            await server.WasShutDown;
        }

        static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.LiterateConsole()
                .CreateLogger();
        }
    }
}
