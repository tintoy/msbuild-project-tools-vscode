using OmniSharp.Extensions.LanguageServer.Models;
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
        ///     Create a new <see cref="CompletionProvider"/>.
        /// </summary>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        protected CompletionProvider(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Log = logger.ForContext(GetType())
                        .ForContext("CompletionProvider", Name);
        }

        /// <summary>
        ///     The provider display name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     The sort priority for the provider's completion items.
        /// </summary>
        public virtual int Priority => 1000;

        /// <summary>
        ///     The provider logger.
        /// </summary>
        protected ILogger Log { get; }

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
