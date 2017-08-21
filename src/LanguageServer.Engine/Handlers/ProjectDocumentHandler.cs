using Lsp;
using Lsp.Capabilities.Server;
using Lsp.Models;
using Lsp.Protocol;
using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.Configuration;
using NuGet.Versioning;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.Handlers
{
    using ContentProviders;
    using Documents;
    using MSBuild;
    using Utilities;

    /// <summary>
    ///     Handler for project file document events.
    /// </summary>
    public class ProjectDocumentHandler
        : TextDocumentHandler
    {
        /// <summary>
        ///     Create a new <see cref="ProjectDocumentHandler"/>.
        /// </summary>
        /// <param name="server">
        ///     The language server.
        /// </param>
        /// <param name="workspace">
        ///     The document workspace.
        /// </param>
        /// <param name="configuration">
        ///     The language server configuration.
        /// </param>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public ProjectDocumentHandler(ILanguageServer server, Workspace workspace, Configuration configuration, ILogger logger)
            : base(server, logger)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            Workspace = workspace;
            Configuration = configuration;
        }

        /// <summary>
        ///     The document selector that describes documents targeted by the handler.
        /// </summary>
        protected override DocumentSelector DocumentSelector => new DocumentSelector(
            new DocumentFilter
            {
                Pattern = "**/*.*",
                Language = "msbuild"
            },
            new DocumentFilter
            {
                Pattern = "**/*.*proj",
                Language = "xml"
            },
            new DocumentFilter
            {
                Pattern = "**/*.props",
                Language = "xml"
            },
            new DocumentFilter
            {
                Pattern = "**/*.targets",
                Language = "xml"
            }
        );

        /// <summary>
        ///     The document workspace.
        /// </summary>
        Workspace Workspace { get; }

        /// <summary>
        ///     The language server configuration.
        /// </summary>
        Configuration Configuration { get; }

        /// <summary>
        ///     Get attributes for the specified text document.
        /// </summary>
        /// <param name="documentUri">
        ///     The document URI.
        /// </param>
        /// <returns>
        ///     The document attributes.
        /// </returns>
        protected override TextDocumentAttributes GetTextDocumentAttributes(Uri documentUri)
        {
            string documentFilePath = VSCodeDocumentUri.GetFileSystemPath(documentUri);
            if (documentFilePath == null)
                return base.GetTextDocumentAttributes(documentUri);

            string extension = Path.GetExtension(documentFilePath).ToLower();
            switch (extension)
            {
                case "props":
                case "targets":
                {
                    break;
                }
                default:
                {
                    if (extension.EndsWith("proj"))
                        break;

                    return base.GetTextDocumentAttributes(documentUri);
                }
            }

            return new TextDocumentAttributes(documentUri, "msbuild");
        }

        /// <summary>
        ///     Called when a definition is requested.
        /// </summary>
        /// <param name="parameters">
        ///     The request parameters.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the request.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation whose result is the definition location or <c>null</c> if no definition is provided.
        /// </returns>
        protected override async Task<LocationOrLocations> OnDefinition(TextDocumentPositionParams parameters, CancellationToken cancellationToken)
        {
            ProjectDocument projectDocument = await Workspace.GetProjectDocument(parameters.TextDocument.Uri);

            using (await projectDocument.Lock.ReaderLockAsync(cancellationToken))
            {
                if (!projectDocument.HasMSBuildProject)
                    return null;

                Position position = parameters.Position.ToNative();
                MSBuildObject msbuildObjectAtPosition = projectDocument.GetMSBuildObjectAtPosition(position);
                if (msbuildObjectAtPosition == null)
                    return null;

                if (msbuildObjectAtPosition is MSBuildSdkImport import)
                {
                    if (msbuildObjectAtPosition is MSBuildSdkImport sdkImportAtPosition)
                    {
                        // TODO: Parse imported project and determine location of root element (use that range instead).
                        Location[] locations =
                            sdkImportAtPosition.ImportedProjectRoots.Select(
                                importedProjectRoot => new Location
                                {
                                    Range = Range.Empty.ToLsp(),
                                    Uri = VSCodeDocumentUri.CreateFromFileSystemPath(importedProjectRoot.Location.File)
                                }
                            )
                            .ToArray();

                        return new LocationOrLocations(locations);
                    }
                    else if (msbuildObjectAtPosition is MSBuildImport importAtPosition)
                    {
                        // TODO: Parse imported project and determine location of root element (use that range instead).
                        return new LocationOrLocations(
                            importAtPosition.ImportedProjectRoots.Select(
                                importedProjectRoot => new Location
                            {
                                Range = Range.Empty.ToLsp(),
                                Uri = VSCodeDocumentUri.CreateFromFileSystemPath(importedProjectRoot.Location.File)
                            }
                        ));
                    }
                }
            }

            return null;
        }
    }
}
