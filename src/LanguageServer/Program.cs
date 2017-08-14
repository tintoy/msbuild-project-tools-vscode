using System;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer
{
    using System.IO;
    using Handlers;

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
            File.WriteAllText("D:\\Stage\\ServerLaunched.txt", "Launched!");

            System.Diagnostics.Debugger.Break();

            SynchronizationContext.SetSynchronizationContext(
                new SynchronizationContext()
            );

            try
            {
                AsyncMain().Wait();
            }
            catch (AggregateException aggregateError)
            {
                int count = 0;
                foreach (Exception unexpectedError in aggregateError.Flatten().InnerExceptions)
                {
                    Console.WriteLine(unexpectedError);

                    File.WriteAllText($"D:\\Stage\\AggregateUnexpectedError{++count}.txt", unexpectedError.ToString());
                }
            }
            catch (Exception unexpectedError)
            {
                Console.WriteLine(unexpectedError);

                File.WriteAllText("D:\\Stage\\UnexpectedError.txt", unexpectedError.ToString());
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

            server.AddHandler(
                new ProjectDocumentHandler(server)
            );

            await server.Initialize();
            await server.WasShutDown;
        }
    }
}
