using Lsp.Protocol;
using Serilog;
using Serilog.Core;
using System;
using Serilog.Events;
using Lsp.Models;

namespace MSBuildProjectTools.LanguageServer.Logging
{
    using Handlers;

    /// <summary>
    ///     A Serilog logging sink that sends log events to the language server logging facility.
    /// </summary>
    public class LanguageServerLoggingSink
        : ILogEventSink
    {
        /// <summary>
        ///     The language server to which events will be logged.
        /// </summary>
        readonly Lsp.LanguageServer _languageServer;

        /// <summary>
        ///     The <see cref="LoggingLevelSwitch"/> that controls logging.
        /// </summary>
        readonly LoggingLevelSwitch _levelSwitch;

        /// <summary>
        ///     Has the language server shut down?
        /// </summary>
        bool _hasServerShutDown;

        /// <summary>
        ///     Create a new language-server event sink.
        /// </summary>
        /// <param name="languageServer">
        ///     The language server to which events will be logged.
        /// </param>
        /// <param name="levelSwitch">
        ///     The <see cref="LoggingLevelSwitch"/> that controls logging.
        /// </param>
        public LanguageServerLoggingSink(Lsp.LanguageServer languageServer, LoggingLevelSwitch levelSwitch)
        {
            if (languageServer == null)
                throw new ArgumentNullException(nameof(languageServer));

            if (levelSwitch == null)
                throw new ArgumentNullException(nameof(levelSwitch));

            _languageServer = languageServer;
            _levelSwitch = levelSwitch;

            _languageServer.Shutdown += shutDownRequested =>
            {
                Log.CloseAndFlush();

                _hasServerShutDown = true;
            };
        }

        /// <summary>
        ///     Emit a log event.
        /// </summary>
        /// <param name="logEvent">
        ///     The log event information.
        /// </param>
        public void Emit(LogEvent logEvent)
        {
            if (_hasServerShutDown)
                return;

            if (logEvent.Level < _levelSwitch.MinimumLevel)
                return;

            LogMessageParams logParameters = new LogMessageParams
            {
                Message = logEvent.RenderMessage()
            };
            if (logEvent.Exception != null)
                logParameters.Message += "\n" + logEvent.Exception.ToString();

            switch (logEvent.Level)
            {
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                {
                    logParameters.Type = MessageType.Error;
                    
                    break;
                }
                case LogEventLevel.Warning:
                {
                    logParameters.Type = MessageType.Warning;
                    
                    break;
                }
                case LogEventLevel.Information:
                {
                    logParameters.Type = MessageType.Info;
                    
                    break;
                }
                default:
                {
                    logParameters.Type = MessageType.Log;

                    break;
                }
            }

            _languageServer.LogMessage(logParameters);
        }
    }
}
