using Sprache;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Parsers for MSBuild expression syntax.
    /// </summary>
    static class Parsers
    {
        /// <summary>
        ///     Parse a generic MSBuild list item.
        /// </summary>
        public static readonly Parser<GenericListItem> GenericListItem = Parse.Positioned(
            from item in Tokens.ListChar.Many().Text()
            select new GenericListItem
            {
                Value = item
            }
        );

        /// <summary>
        ///     Parse a generic MSBuild list item.
        /// </summary>
        public static readonly Parser<GenericListSeparator> GenericListSeparator = Parse.Positioned(
            from leading in Parse.WhiteSpace.Many().Text()
            from separator in Tokens.Semicolon
            from trailing in Parse.WhiteSpace.Many().Text()
            select new GenericListSeparator
            {
                SeparatorOffset = leading.Length
            }
        );

        /// <summary>
        ///     Parse a generic MSBuild list's leading item separator.
        /// </summary>
        public static readonly Parser<IEnumerable<ExpressionNode>> GenericListLeadingSeparator =
            from separator in GenericListSeparator.Once()
            let leadingEmptyItem = new GenericListItem
            {
                Value = String.Empty
            }
            select leadingEmptyItem.AsSequence().Concat<ExpressionNode>(separator);

        /// <summary>
        ///     Parse a generic MSBuild list item separator, optionally followed by a generic list item.
        /// </summary>
        public static readonly Parser<IEnumerable<ExpressionNode>> GenericListSeparatorWithItem =
            from separator in GenericListSeparator.Once()
            from item in GenericListItem.Once()
            select separator.Concat<ExpressionNode>(item);

        /// <summary>
        ///     Parse a generic MSBuild list, delimited by semicolons.
        /// </summary>
        public static readonly Parser<GenericList> GenericList = Parse.Positioned(
            from leadingSeparator in GenericListLeadingSeparator.Optional()
            from firstItem in GenericListItem.Once<ExpressionNode>()
            from remainingItems in GenericListSeparatorWithItem.Many()
            let items =
                leadingSeparator.AsFlattenedSequenceIfDefined()
                    .Concat(firstItem)
                    .Concat(
                        remainingItems.SelectMany(items => items)
                    )
            select new GenericList
            {
                Children = ImmutableList.CreateRange(items)
            }
        );

        static IEnumerable<T> AsSequence<T>(this T item)
        {
            yield return item;
        }

        static IEnumerable<T> AsSequenceIfDefined<T>(this IOption<T> item)
        {
            if (item.IsDefined)
                yield return item.Get();
        }

        static IEnumerable<T> AsFlattenedSequenceIfDefined<T>(this IOption<IEnumerable<T>> items)
        {
            if (items.IsDefined)
            {
                foreach (T item in items.Get())
                    yield return item;
            }
        }

        static IEnumerable<T> AsSequenceOrElse<T>(this IOption<T> item, T valueIfNotDefined)
        {
            yield return item.GetOrElse(valueIfNotDefined);

        }
    }
}
