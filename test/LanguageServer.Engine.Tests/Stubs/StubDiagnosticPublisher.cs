using Lsp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
    
namespace MSBuildProjectTools.LanguageServer.Tests.Stubs
{
    using Diagnostics;

    /// <summary>
    ///     A stub implementation of <see cref="IPublishDiagnostics"/> that captures published diagnostics.
    /// </summary>
    public class StubDiagnosticPublisher
        : IPublishDiagnostics
    {
        /// <summary>
        ///     Create a new <see cref="StubDiagnosticPublisher"/>.
        /// </summary>
        public StubDiagnosticPublisher()
        {
        }

        /// <summary>
        ///     Published diagnostics, keyed by document URI.
        /// </summary>
        public Dictionary<Uri, Diagnostic[]> Diagnostics { get; } = new Dictionary<Uri, Diagnostic[]>();

        /// <summary>
        ///     Publish the specified diagnostics.
        /// </summary>
        /// <param name="documentUri">
        ///     The URI of the document that the diagnostics apply to.
        /// </param>
        /// <param name="diagnostics">
        ///     A sequence of <see cref="Diagnostic"/>s to publish.
        /// </param>
        public void Publish(Uri documentUri, IEnumerable<Diagnostic> diagnostics)
        {
            if (documentUri == null)
                throw new ArgumentNullException(nameof(documentUri));
            
            if (diagnostics != null && diagnostics.Any())
                Diagnostics[documentUri] = diagnostics.ToArray();
            else
                Diagnostics.Remove(documentUri);
        }
    }
}
