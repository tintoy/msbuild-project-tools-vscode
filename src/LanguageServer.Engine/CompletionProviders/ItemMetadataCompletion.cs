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
    ///     Completion provider for metadata elements / attributes of items.
    /// </summary>
    public class ItemMetadataCompletion
        : CompletionProvider
    {
        /// <summary>
        ///     Create a new <see cref="ItemMetadataCompletion"/>.
        /// </summary>
        /// <param name="logger">
        ///     The application logger.
        /// </param>
        public ItemMetadataCompletion(ILogger logger)
            : base(logger)
        {
        }

        /// <summary>
        ///     The provider display name.
        /// </summary>
        public override string Name => "Item Metadata";

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

            Log.Verbose("Evaluate completions for {XmlLocation:l}", location);

            List<CompletionItem> completions = new List<CompletionItem>();

            using (await projectDocument.Lock.ReaderLockAsync())
            {
                completions.AddRange(
                    GetAttributeCompletions(location, projectDocument)
                );

                completions.AddRange(
                    GetElementCompletions(location, projectDocument)
                );
            }

            if (completions.Count == 0)
                return null; // No completions offered

            Log.Verbose("Offering {CompletionCount} completions for {XmlLocation:l}.", location);

            return new CompletionList(completions,
                isIncomplete: false // List is exhaustive
            );
        }

        IEnumerable<CompletionItem> GetAttributeCompletions(XmlLocation location, ProjectDocument projectDocument)
        {
            Log.Verbose("Evaluate attribute completions for {XmlLocation:l}", location);

            XSElement element;
            if (!location.IsElementBetweenAttributes(out element))
            {
                Log.Verbose("Not offering any attribute completions for {XmlLocation:l} (not a location where we can offer attribute completion.", location);

                yield break;
            }

            // Must be an item element.
            // TODO: Make an XmlLocation.IsItemElement extension method for this.
            if (element.ParentElement?.Name != "ItemGroup")
            {
                Log.Verbose("Not offering any attribute completions for {XmlLocation:l} (element is not a direct child of a 'PropertyGroup' element).", location);

                yield break;
            }

            // These items are handled by PackageReferenceCompletion.
            if (element.Name == "PackageReference" || element.Name == "DotNetCliToolReference")
            {
                Log.Verbose("Not offering any attribute completions for {XmlLocation:l} ({ItemType} items are handled by another provider).",
                    location,
                    element.Name
                );

                yield break;
            }

            string itemType = element.Name;
            if (MSBuildSchemaHelp.ForItemType(itemType) == null)
            {
                Log.Verbose("Not offering any attribute completions for {XmlLocation:l} ({ItemType} is not a well-known item type).",
                    location,
                    itemType
                );

                yield break;
            }

            Log.Verbose("Will offer attribute completions for {XmlLocation:l}", location);

            const string universalMetadataPrefix = "*.";
            string metadataPrefix = String.Format("{0}.", itemType);
            foreach (string metadataKey in MSBuildSchemaHelp.WellKnownItemMetadata) // TODO: Find a better way to capture well-known item types / metadata names
            {
                string metadataName;
                if (metadataKey.StartsWith(metadataPrefix))
                    metadataName = metadataKey.Substring(metadataPrefix.Length);
                else if (metadataKey.StartsWith(universalMetadataPrefix))
                    metadataName = metadataKey.Substring(universalMetadataPrefix.Length);
                else
                    continue;

                yield return new CompletionItem
                {
                    Label = metadataName,
                    Documentation = MSBuildSchemaHelp.ForItemMetadata(itemType, metadataName),
                    SortText = Priority + metadataName,
                    TextEdit = new TextEdit
                    {
                        NewText = $"{metadataName}=\"$0\" ",
                        Range = location.Position.ToEmptyRange().ToLsp()
                    },
                    InsertTextFormat = InsertTextFormat.Snippet
                };
            }
        }

        IEnumerable<CompletionItem> GetElementCompletions(XmlLocation location, ProjectDocument projectDocument)
        {
            Log.Verbose("Evaluate element completions for {XmlLocation:l}", location);

            XSElement element;
            if (!location.CanCompleteElement(out element))
            {
                Log.Verbose("Not offering any element completions for {XmlLocation:l} (not a location where we can offer element completion.", location);

                yield break;
            }

            // Must be an item element.
            // TODO: Make an XmlLocation.IsItemElement extension method for this.
            if (element.ParentElement?.ParentElement?.Name != "ItemGroup")
            {
                Log.Verbose("Not offering any element completions for {XmlLocation:l} (parent element {ParentElementName} has parent {ParentParentElementName}, rather than 'PropertyGroup').",
                    location,
                    element.ParentElement?.Name,
                    element.ParentElement?.ParentElement?.Name
                );

                yield break;
            }

            // These items are handled by PackageReferenceCompletion.
            if (element.ParentElement?.Name == "PackageReference" || element.ParentElement?.Name == "DotNetCliToolReference")
            {
                Log.Verbose("Not offering any element completions for {XmlLocation:l} ({ItemType} items are handled by another provider).",
                    location,
                    element.Name
                );

                yield break;
            }

            string itemType = element.ParentElement.Name;
            if (MSBuildSchemaHelp.ForItemType(itemType) == null)
            {
                Log.Verbose("Not offering any element completions for {XmlLocation:l} ({ItemType} is not a well-known item type).",
                    location,
                    itemType
                );

                yield break;
            }

            Log.Verbose("Will offer element completions for {XmlLocation:l}", location);

            const string universalMetadataPrefix = "*.";
            string metadataPrefix = String.Format("{0}.", itemType);
            foreach (string metadataKey in MSBuildSchemaHelp.WellKnownItemMetadata) // TODO: Find a better way to capture well-known item types / metadata names
            {
                string metadataName;
                if (metadataKey.StartsWith(metadataPrefix))
                    metadataName = metadataKey.Substring(metadataPrefix.Length);
                else if (metadataKey.StartsWith(universalMetadataPrefix))
                    metadataName = metadataKey.Substring(universalMetadataPrefix.Length);
                else
                    continue;

                string completionLabel = $"<{metadataName}>";

                yield return new CompletionItem
                {
                    Label = completionLabel,
                    Documentation = MSBuildSchemaHelp.ForItemMetadata(itemType, metadataName),
                    SortText = Priority + completionLabel,
                    TextEdit = new TextEdit
                    {
                        NewText = $"<{metadataName}>$0</{metadataName}>",
                        Range = element.Range.ToLsp()
                    },
                    InsertTextFormat = InsertTextFormat.Snippet
                };
            }
        }
    }
}
