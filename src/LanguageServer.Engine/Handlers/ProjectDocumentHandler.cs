using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
    
namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using System.Collections.Generic;
    using Utilities;
    using XmlParser;

    /// <summary>
    ///     Handler for project file document events.
    /// </summary>
    public class ProjectDocumentHandler
        : TextDocumentHandler
    {
        /// <summary>
        ///     XML for loaded project files.
        /// </summary>
        readonly Dictionary<string, XDocument> _projectXml = new Dictionary<string, XDocument>();

        /// <summary>
        ///     Create a new <see cref="ProjectDocumentHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public ProjectDocumentHandler(ILanguageServer server, ILogger logger)
            : base(server, logger)
        {
            Options.Change = TextDocumentSyncKind.Incremental;
        }

        /// <summary>
        ///     The document selector that describes documents targeted by the handler.
        /// </summary>
        protected override DocumentSelector DocumentSelector => new DocumentSelector(
            new DocumentFilter
            {
                Pattern = "**/*.*proj",
                Language = "xml"
            }
        );

        /// <summary>
        ///     Called when a text document is opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override Task OnDidOpenTextDocument(DidOpenTextDocumentParams parameters)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return Task.CompletedTask;

            Logger.Information("Opened project file '{DocumentPath}'.", documentPath);

            XDocument projectXml = ParseProjectXml(documentPath);
            if (projectXml != null)
            {
                _projectXml.Add(documentPath, projectXml);

                XElement[] packageReferenceElements = projectXml.Descendants("PackageReference").ToArray();
                Logger.Information("Successfully parsed XML for project '{DocumentPath}' ({PackageReferenceCount} package references detected).",
                    documentPath,
                    packageReferenceElements.Length
                );
                foreach (XElement packageReferenceElement in packageReferenceElements)
                {
                    Location elementLocation = packageReferenceElement.Annotation<Location>();
                    if (elementLocation == null)
                        continue;

                    Logger.Information("Found PackageReference element spanning ({StartLine},{StartColumn}) to ({EndLine},{EndColumn}).",
                        elementLocation.Start.LineNumber,
                        elementLocation.Start.ColumnNumber,
                        elementLocation.End.LineNumber,
                        elementLocation.End.ColumnNumber
                    );
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called when a text document is opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override Task OnDidCloseTextDocument(DidCloseTextDocumentParams parameters)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return Task.CompletedTask;

            _projectXml.Remove(documentPath);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called when a text document is opened.
        /// </summary>
        /// <param name="parameters">
        ///     The notification parameters.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        protected override Task OnDidChangeTextDocument(DidChangeTextDocumentParams parameters)
        {
            string documentPath = parameters.TextDocument.Uri.GetFileSystemPath();
            if (documentPath == null)
                return Task.CompletedTask;

            Logger.Information("Document '{DocumentPath}' changed ({ChangeCount} changes detected).",
                documentPath,
                parameters.ContentChanges.Count()
            );
            foreach (TextDocumentContentChangeEvent change in parameters.ContentChanges)
            {
                Logger.Verbose("Changed {RangeLength} characters in '{DocumentPath}' between ({StartRangeLine}, {StartRangeColumn}) to ({EndRangeLine}, {EndRangeColumn}).",
                    change.RangeLength,
                    documentPath,
                    change.Range?.Start?.Line ?? 0,
                    change.Range?.Start.Character ?? 0,
                    change.Range?.End?.Line ?? 0,
                    change.Range?.End?.Character ?? 0
                );
                Logger.Verbose("ChangeEvent: {EventData}", JsonConvert.SerializeObject(change));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Parse a project file's XML.
        /// </summary>
        /// <param name="filePath">
        ///     The project file path.
        /// </param>
        /// <returns>
        ///     An <see cref="XDocument"/> representing the project XML, or <c>null</c> if the project file could not be loaded / parsed.
        /// </returns>
        XDocument ParseProjectXml(string filePath)
        {
            try
            {
                return LocatingXmlTextReader.LoadWithLocations(filePath);
            }
            catch (XmlException invalidXml)
            {
                Logger.Error("Error parsing project file '{FilePath}': {ErrorMessage}",
                    filePath,
                    invalidXml.Message
                );
            }
            catch (Exception loadError)
            {
                Logger.Error(loadError, "Unexpected error loading file {FilePath}.", filePath);
            }

            return null;
        }
    }
}
