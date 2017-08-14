using Lsp;
using Lsp.Capabilities.Client;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Serilog;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer
{
    class TextDocumentHandler
        : ITextDocumentSyncHandler
    {
        readonly ILanguageServer _router;
        readonly ILogger _log;

        readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter
            {
                Pattern = "**/*.csproj",
                Language = "xml"
            }
        );

        SynchronizationCapability _capability;

        public TextDocumentHandler(ILanguageServer router)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));
            
            _router = router;
            _log = Log.Logger.ForContext<TextDocumentHandler>();
        }

        public TextDocumentSyncOptions Options { get; } = new TextDocumentSyncOptions
        {
            WillSaveWaitUntil = false,
            WillSave = true,
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions
            {
                IncludeText = true
            },
            OpenClose = true
        };

        public Task Handle(DidChangeTextDocumentParams notification)
        {
            string documentPath = notification.TextDocument.Uri.GetFileSystemPath();
            int changeCount = notification.ContentChanges.Count();
            _router.LogMessage(new LogMessageParams
            {
                Type = MessageType.Log,
                Message = $"Document '{documentPath}' changed."
            });
            foreach (TextDocumentContentChangeEvent change in notification.ContentChanges)
            {
                _router.LogMessage(new LogMessageParams
                {
                    Type = MessageType.Log,
                    Message = $"Changed {change.RangeLength} characters in '{documentPath}' between ({change.Range.Start.Line}, {change.Range.Start.Character}) to ({change.Range.End.Line}, {change.Range.End.Character})."
                });
            }

            return Task.CompletedTask;
        }

        public Task Handle(DidOpenTextDocumentParams notification)
        {
            string documentPath = notification.TextDocument.Uri.GetFileSystemPath();
            _router.LogMessage(new LogMessageParams
            {
                Type = MessageType.Log,
                Message = $"Opened document '{documentPath}'."
            });

            return Task.CompletedTask;
        }

        public Task Handle(DidCloseTextDocumentParams notification)
        {
            return Task.CompletedTask;
        }

        public Task Handle(DidSaveTextDocumentParams notification)
        {
            return Task.CompletedTask;
        }

        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                SyncKind = Options.Change
            };
        }

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector
            };
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                IncludeText = Options.Save.IncludeText
            };
        }
        public void SetCapability(SynchronizationCapability capability)
        {
            _capability = capability;
        }


        public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
        {
            return new TextDocumentAttributes(uri, "csharp");
        }

        void LogInformation(string message, params object[] propertyValues)
        {
            _log.Information(message, propertyValues);

            // TODO: _router.LogMessage
        }

        void LogWarning(string message, params object[] propertyValues)
        {
            _log.Warning(message, propertyValues);

            // TODO: _router.LogMessage
        }

        void LogError(string message, params object[] propertyValues)
        {
            _log.Error(message, propertyValues);

            // TODO: _router.LogMessage
        }

        void LogError(Exception exception, string message, params object[] propertyValues)
        {
            _log.Error(exception, message, propertyValues);

            // TODO: _router.LogMessage
        }
    }

    static class UriExtensions
    {
        public static string GetFileSystemPath(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            
            string path = uri.LocalPath;
            if (Path.DirectorySeparatorChar == '\\' && path.StartsWith('/'))
                path = path.Substring(1).Replace('/', '\\');

            return path;
        }
    }
}
