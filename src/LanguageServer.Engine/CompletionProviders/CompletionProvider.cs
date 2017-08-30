using Lsp.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer.CompletionProviders
{
    using Documents;
    using SemanticModel;

    /// <summary>
    ///     The base class for completion providers.
    /// </summary>
    public abstract class CompletionProvider
        : ICompletionProvider
    {
        /// <summary>
        ///     A dummy completion list to indicate that a provider offers no completions, but may offer completions for future iterations of the current completion (i.e. as the user continues to type).
        /// </summary>
        public static readonly CompletionList CallMeAgain = new CompletionList(
            new CompletionItem[]
            {
                new CompletionItem
                {
                    Label = "Dummy",
                    InsertText = "This should never be displayed."
                }
            },
            isIncomplete: true
        );

        /// <summary>
        ///     Create a new <see cref="CompletionProvider"/>.
        /// </summary>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        protected CompletionProvider(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Log = logger.ForContext("CompletionProvider", GetType().FullName);
        }

        /// <summary>
        ///     The provider logger.
        /// </summary>
        ILogger Log { get; }

        /// <summary>
        ///     Provide completions for the specified location.
        /// </summary>
        /// <param name="location">
        ///     The <see cref="XmlLocation"/> where completions are requested.
        /// </param>
        /// <param name="projectDocument">
        ///     The <see cref="ProjectDocument"/> that contains the <paramref name="location"/>.
        /// </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> that resolves either a <see cref="CompletionList"/>, or <c>null</c> if no completions are provided.
        /// </returns>
        public abstract Task<CompletionList> ProvideCompletions(XmlLocation location, ProjectDocument projectDocument, CancellationToken cancellationToken = default(CancellationToken));
    }
}
