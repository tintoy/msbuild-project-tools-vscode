using Serilog;
using Serilog.Core;
using System;
using Serilog.Events;
using Xunit.Abstractions;

namespace MSBuildProjectTools.LanguageServer.Tests.Logging
{
    /// <summary>
    ///     A Serilog logging sink that sends log events to the language server logging facility.
    /// </summary>
    public class TestOutputLoggingSink
        : ILogEventSink
    {
        /// <summary>
        ///     The language server to which events will be logged.
        /// </summary>
        readonly ITestOutputHelper _testOutput;

        /// <summary>
        ///     The <see cref="LoggingLevelSwitch"/> that controls logging.
        /// </summary>
        readonly LoggingLevelSwitch _levelSwitch;

        /// <summary>
        ///     Create a new language-server event sink.
        /// </summary>
        /// <param name="testOutput">
        ///     The language server to which events will be logged.
        /// </param>
        /// <param name="levelSwitch">
        ///     The <see cref="LoggingLevelSwitch"/> that controls logging.
        /// </param>
        public TestOutputLoggingSink(ITestOutputHelper testOutput, LoggingLevelSwitch levelSwitch)
        {
            if (testOutput == null)
                throw new ArgumentNullException(nameof(testOutput));

            if (levelSwitch == null)
                throw new ArgumentNullException(nameof(levelSwitch));

            _testOutput = testOutput;
            _levelSwitch = levelSwitch;
        }

        /// <summary>
        ///     Emit a log event.
        /// </summary>
        /// <param name="logEvent">
        ///     The log event information.
        /// </param>
        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level < _levelSwitch.MinimumLevel)
                return;

            string message = logEvent.RenderMessage();
            if (logEvent.Exception != null)
                message += "\n" + logEvent.Exception.ToString();

            _testOutput.WriteLine(message);
        }
    }
}
