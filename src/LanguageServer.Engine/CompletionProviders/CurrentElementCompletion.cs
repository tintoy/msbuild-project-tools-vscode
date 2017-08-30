using Lsp.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer.CompletionProviders
{
    using Documents;
    using SemanticModel;
    using Utilities;

    /// <summary>
    ///     Completion provider for the current element.
    /// </summary>
    public class CurrentElementCompletion
        : CompletionProvider
    {
        /// <summary>
        ///     Create a new <see cref="CurrentElementCompletion"/>.
        /// </summary>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public CurrentElementCompletion(ILogger logger)
            : base(logger)
        {
        }

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
        ///     A <see cref="Task{TResult}"/> that resolves either a <see cref="CompletionList"/>s, or <c>null</c> if no completions are provided.
        /// </returns>
        public override async Task<CompletionList> ProvideCompletions(XmlLocation location, ProjectDocument projectDocument, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (projectDocument == null)
                throw new ArgumentNullException(nameof(projectDocument));

            List<CompletionItem> completions = new List<CompletionItem>();

            Log.Verbose("Evaluate completions for {XmlLocation:l}", location);

            using (await projectDocument.Lock.ReaderLockAsync())
            {
                if (location.CanCompleteElement(out XSElement replaceElement))
                {
                    if (replaceElement != null && !String.IsNullOrWhiteSpace(replaceElement.Name))
                    {
                        Log.Verbose("Offering completion to replace element {ElementName} @ {ReplaceRange:l}",
                            replaceElement.Name,
                            replaceElement.Range
                        );
                    }
                    else
                    {
                        Log.Verbose("Element @ {ElementRange} is empty; completion will not be offered (but will indicate that future iterations of this completion could succeed).",
                            replaceElement.Range
                        );

                        return CallMeAgain;
                    }

                    string elementText = $"<{replaceElement.Name} />";

                    completions.Add(new CompletionItem
                    {
                        Label = elementText,
                        SortText = $"500{elementText}",
                        Kind = CompletionItemKind.Text,
                        TextEdit = new TextEdit
                        {
                            NewText = elementText,
                            Range = replaceElement.Range.ToLsp()
                        }
                    });
                }
                else
                    Log.Verbose("Not offering any completions for {XmlLocation:l}", location);
            }

            Log.Verbose("Offering {CompletionCount} completion(s) for {XmlLocation:l}", completions.Count, location);

            if (completions.Count == 0)
                return null;

            return new CompletionList(completions,
                isIncomplete: true // AF: Consider the performance implications if other providers take a while to offer completions; at that point, we may want to consider switching to using "resolve"-style completion.
            );
        }
    }
}
