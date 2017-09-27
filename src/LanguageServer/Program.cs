using Autofac;
using OmniSharp.Extensions.LanguageServer;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using LSP = OmniSharp.Extensions.LanguageServer;

namespace MSBuildProjectTools.LanguageServer
{
    using Documents;
    using Handlers;
    using Logging;
    using Utilities;

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
                AutoDetectExtensionDirectory();

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
            using (ActivityCorrelationManager.BeginActivityScope())
            using (IContainer container = BuildContainer())
            {
                var server = container.Resolve<LSP.LanguageServer>();

                await server.Initialize();
                await server.WasShutDown;
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
        ///     Auto-detect the directory containing the extension's files.
        /// </summary>
        static void AutoDetectExtensionDirectory()
        {
            string extensionDir = Environment.GetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_DIR");
            if (String.IsNullOrWhiteSpace(extensionDir))
            {
                extensionDir = Path.Combine(
                    Path.GetDirectoryName(typeof(Program).Assembly.Location), "..", ".."
                );
            }
            extensionDir = Path.GetFullPath(extensionDir);
            Environment.SetEnvironmentVariable("MSBUILD_PROJECT_TOOLS_DIR", extensionDir);
        }
    }
}
