using Serilog;
using Serilog.Context;
using Serilog.Core;
using System;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using LanguageServer.Logging;
    using Logging;

    /// <summary>
    ///     The base class for test suites.
    /// </summary>
    public abstract class TestBase
        : IDisposable
    {
        /// <summary>
        ///     An <see cref="IDisposable"/> representing the log context for the current test.
        /// </summary>
        readonly IDisposable _logContext;

        /// <summary>
        ///     Create a new test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        protected TestBase(ITestOutputHelper testOutput)
        {
            if (testOutput == null)
                throw new ArgumentNullException(nameof(testOutput));

            TestOutput = testOutput;

            // Redirect component logging to Serilog.
            Log =
                new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.WithCurrentActivityId()
                    .Enrich.FromLogContext()
                    .WriteTo.TestOutput(TestOutput, LogLevelSwitch)
                    .CreateLogger();

            // Ugly hack to get access to the current test.
            CurrentTest =
                (ITest)TestOutput.GetType()
                    .GetField("test", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(TestOutput);

            Assert.True(CurrentTest != null, "Cannot retrieve current test from ITestOutputHelper.");

            _logContext = LogContext.PushProperty("TestName", CurrentTest.DisplayName);
        }

        /// <summary>
        ///     Dispose of resources being used by the test.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     Output for the current test.
        /// </summary>
        protected ITestOutputHelper TestOutput { get; }

        /// <summary>
        ///     A <see cref="ITest"/> representing the current test.
        /// </summary>
        protected ITest CurrentTest { get; }

        /// <summary>
        ///     The Serilog logger for the current test.
        /// </summary>
        protected ILogger Log { get; }

        /// <summary>
        ///     A switch to control the logging level for the current test.
        /// </summary>
        protected LoggingLevelSwitch LogLevelSwitch { get; } = new LoggingLevelSwitch();
    }
}
