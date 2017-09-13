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
    using SemanticModel.MSBuildExpressions;
    using Utilities;

    /// <summary>
    ///     Completion provider for item metadata expressions.
    /// </summary>
    public class ItemMetadataExpressionCompletion
        : CompletionProvider
    {
        /// <summary>
        ///     Create a new <see cref="ItemMetadataExpressionCompletion"/>.
        /// </summary>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public ItemMetadataExpressionCompletion(ILogger logger)
            : base(logger)
        {
        }

        /// <summary>
        ///     The provider display name.
        /// </summary>
        public override string Name => "ItemMetadata Expressions";

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
                if (!projectDocument.EnableExpressions)
                    return null;

                ExpressionNode expression;
                Range expressionRange;
                if (!location.IsExpression(out expression, out expressionRange))
                {
                    Log.Verbose("Not offering any completions for {XmlLocation:l} (not on an expression or a location where an expression can be added).", location);

                    return null;
                }

                if (expression is Symbol)
                    expression = expression.Parent; // The containing expression.

                if (expression is ExpressionTree)
                    expressionRange = location.Position.ToEmptyRange(); // We're between expressions, so just insert.
                else if (expression.Kind != ExpressionKind.ItemMetadata)
                {
                    Log.Verbose("Not offering any completions for {XmlLocation:l} (this provider only supports MSBuild ItemMetadata expressions, not {ExpressionKind} expressions).", location, expression.Kind);

                    return null;
                }
                
                Log.Verbose("Offering completions to replace Evaluate expression @ {ReplaceRange:l}",
                    expressionRange
                );

                completions.AddRange(
                    GetCompletionItems(projectDocument, expressionRange)
                );
            }

            Log.Verbose("Offering {CompletionCount} completion(s) for {XmlLocation:l}", completions.Count, location);

            if (completions.Count == 0)
                return null;

            return new CompletionList(completions,
                isIncomplete: false // Consider this list to be exhaustive
            );
        }

        /// <summary>
        ///     Get item metadata element completions.
        /// </summary>
        /// <param name="projectDocument">
        ///     The <see cref="ProjectDocument"/> for which completions will be offered.
        /// </param>
        /// <param name="replaceRange">
        ///     The range of text to be replaced by the completions.
        /// </param>
        /// <returns>
        ///     A sequence of <see cref="CompletionItem"/>s.
        /// </returns>
        public IEnumerable<CompletionItem> GetCompletionItems(ProjectDocument projectDocument, Range replaceRange)
        {
            if (projectDocument == null)
                throw new ArgumentNullException(nameof(projectDocument));

            if (replaceRange == null)
                throw new ArgumentNullException(nameof(replaceRange));

            Lsp.Models.Range replaceRangeLsp = replaceRange.ToLsp();

            // Built-in metadata.

            yield return UnqualifiedMetadataCompletionItem("FullPath", replaceRangeLsp,
                description: "Contains the full path of the item."
            );
            yield return UnqualifiedMetadataCompletionItem("RootDir", replaceRangeLsp,
                description: "Contains the root directory of the item."
            );
            yield return UnqualifiedMetadataCompletionItem("Filename", replaceRangeLsp,
                description: "Contains the file name of the item, without the extension."
            );
            yield return UnqualifiedMetadataCompletionItem("Extension", replaceRangeLsp,
                description: "Contains the file name extension of the item."
            );
            yield return UnqualifiedMetadataCompletionItem("RelativeDir", replaceRangeLsp,
                description: "Contains the path specified in the Include attribute, up to the final slash or backslash."
            );
            yield return UnqualifiedMetadataCompletionItem("Directory", replaceRangeLsp,
                description: "Contains the directory of the item, without the root directory."
            );
            yield return UnqualifiedMetadataCompletionItem("RecursiveDir", replaceRangeLsp,
                description: "If the Include attribute contains the wildcard **, this metadata specifies the part of the path that replaces the wildcard."
            );
            yield return UnqualifiedMetadataCompletionItem("Identity", replaceRangeLsp,
                description: "The item specified in the Include attribute."
            );
            yield return UnqualifiedMetadataCompletionItem("ModifiedTime", replaceRangeLsp,
                description: "Contains the timestamp from the last time the item was modified."
            );
            yield return UnqualifiedMetadataCompletionItem("CreatedTime", replaceRangeLsp,
                description: "Contains the timestamp from the last time the item was created."
            );
            yield return UnqualifiedMetadataCompletionItem("AccessedTime", replaceRangeLsp,
                description: "Contains the timestamp from the last time the item was last accessed."
            );

            // Qualified metadata for the current item type (from project state).

            int priority = Priority + 200;

            // TODO: Implement!

            // Qualified metadata for other well-known item types (only if we don't already have an item type).

            priority = Priority + 500;

            const string wellKnownMetadataPrefix = "*.";
            foreach (string metadataKey in MSBuildSchemaHelp.WellKnownItemMetadata) // TODO: Find a better way to capture well-known item types / metadata names
            {
                if (metadataKey.StartsWith(wellKnownMetadataPrefix))
                    continue;

                string[] keySegments = metadataKey.Split(new char[] { '.' }, count: 2);
                string itemType = keySegments[0];
                string metadataName = keySegments[1];

                yield return QualifiedMetadataCompletionItem(itemType, metadataName, replaceRangeLsp, priority,
                    description: MSBuildSchemaHelp.ForItemMetadata(itemType, metadataName)
                );
            }
        }

        /// <summary>
        ///     Create a standard <see cref="CompletionItem"/> for the specified unqualified MSBuild item metadata.
        /// </summary>
        /// <param name="metadataName">
        ///     The MSBuild item metadata name.
        /// </param>
        /// <param name="replaceRange">
        ///     The range of text that will be replaced by the completion.
        /// </param>
        /// <param name="priority">
        ///     The item sort priority (defaults to <see cref="CompletionProvider.Priority"/>).
        /// </param>
        /// <param name="description">
        ///     An optional description for the item.
        /// </param>
        /// <returns>
        ///     The <see cref="CompletionItem"/>.
        /// </returns>
        CompletionItem UnqualifiedMetadataCompletionItem(string metadataName, Lsp.Models.Range replaceRange, int? priority = null, string description = null)
        {
            return new CompletionItem
            {
                Label = $"%({metadataName})",
                Documentation = description,
                FilterText = $"%({metadataName}.)", // Trailing "." ensures the user can type "." to switch to qualified item metadata expression without breaking completion.
                SortText = $"{priority ?? Priority}%({metadataName})",
                TextEdit = new TextEdit
                {
                    NewText = $"%({metadataName})",
                    Range = replaceRange
                }
            };
        }

        /// <summary>
        ///     Create a standard <see cref="CompletionItem"/> for the specified qualified MSBuild item metadata.
        /// </summary>
        /// <param name="itemType">
        ///     The MSBuild item type.
        /// </param>
        /// <param name="metadataName">
        ///     The MSBuild item metadata name.
        /// </param>
        /// <param name="replaceRange">
        ///     The range of text that will be replaced by the completion.
        /// </param>
        /// <param name="priority">
        ///     The item sort priority (defaults to <see cref="CompletionProvider.Priority"/>).
        /// </param>
        /// <param name="description">
        ///     An optional description for the item.
        /// </param>
        /// <returns>
        ///     The <see cref="CompletionItem"/>.
        /// </returns>
        CompletionItem QualifiedMetadataCompletionItem(string itemType, string metadataName, Lsp.Models.Range replaceRange, int? priority = null, string description = null)
        {
            return new CompletionItem
            {
                Label = $"%({itemType}.{metadataName})",
                Documentation = description,
                SortText = $"{priority ?? Priority}%({itemType}.{metadataName})",
                TextEdit = new TextEdit
                {
                    NewText = $"%({itemType}.{metadataName})",
                    Range = replaceRange
                }
            };
        }
    }
}
