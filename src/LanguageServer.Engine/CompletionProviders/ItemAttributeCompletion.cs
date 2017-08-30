using Lsp.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MSBuildProjectTools.LanguageServer.CompletionProviders
{
    using Documents;
    using SemanticModel;
    using Utilities;

    /// <summary>
    ///     Completion provider for attributes of items.
    /// </summary>
    public class ItemAttributeCompletion
        : CompletionProvider
    {
        /// <summary>
        ///     The names of well-known attributes for MSBuild item elements.
        /// </summary>
        public static readonly ImmutableHashSet<string> WellKnownItemAttributes =
            ImmutableHashSet.CreateRange(new string[]
            {
                "Include",
                "Condition",
                "Exclude",
                "Update"
            });

        /// <summary>
        ///     Create a new <see cref="ItemAttributeCompletion"/>.
        /// </summary>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public ItemAttributeCompletion(ILogger logger)
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

            using (await projectDocument.Lock.ReaderLockAsync())
            {
                XSElement element;
                if (!location.IsElementBetweenAttributes(out element))
                    return null;

                // Must be an item element.
                // TODO: Make an XmlLocation.IsItemElement extension method for this.
                if (element.ParentElement?.Name != "ItemGroup")
                    return null;

                // These items are handled by PackageReferenceCompletion.
                if (element.Name == "PackageReference" || element.Name == "DotNetCliToolReference")
                    return null;

                // TODO: Check if we need to add a leading " " because we're directly after an attribute.

                Lsp.Models.Range insertRange = location.Position.ToEmptyRange().ToLsp();
                
                completions.AddRange(
                    WellKnownItemAttributes.Except(
                        element.AttributeNames
                    )
                    .Select(attributeName => new CompletionItem
                    {
                        Label = attributeName,
                        Kind = CompletionItemKind.Field,
                        TextEdit = new TextEdit
                        {
                            NewText = $"{attributeName}=\"$1\"$0",
                            Range = insertRange
                        },
                        InsertTextFormat = InsertTextFormat.Snippet
                    })
                );
            }

            if (completions.Count == 0)
                return null;

            return new CompletionList(completions, isIncomplete: false);
        }
    }
}
