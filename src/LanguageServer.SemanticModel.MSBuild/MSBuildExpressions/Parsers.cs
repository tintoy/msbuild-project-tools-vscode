using Sprache;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    using Utilities;

    /// <summary>
    ///     Parsers for MSBuild expression syntax.
    /// </summary>
    static class Parsers
    {
        /// <summary>
        ///     Parse a generic MSBuild list item.
        /// </summary>
        public static readonly Parser<SimpleListItem> SimpleListItem = Parse.Positioned(
            from item in Tokens.ListChar.Many().Text()
            select new SimpleListItem
            {
                Value = item
            }
        );

        /// <summary>
        ///     Parse a generic MSBuild list item.
        /// </summary>
        public static readonly Parser<SimpleListSeparator> SimpleListSeparator = Parse.Positioned(
            from leading in Parse.WhiteSpace.Many().Text()
            from separator in Tokens.Semicolon
            from trailing in Parse.WhiteSpace.Many().Text()
            select new SimpleListSeparator
            {
                SeparatorOffset = leading.Length
            }
        );

        /// <summary>
        ///     Parse a generic MSBuild list's leading item separator.
        /// </summary>
        public static readonly Parser<IEnumerable<ExpressionNode>> SimpleListLeadingSeparator =
            from separator in SimpleListSeparator.Once()
            let leadingEmptyItem = new SimpleListItem
            {
                Value = String.Empty
            }
            select leadingEmptyItem.ToSequence().Concat<ExpressionNode>(separator);

        /// <summary>
        ///     Parse a generic MSBuild list item separator, optionally followed by a simple list item.
        /// </summary>
        public static readonly Parser<IEnumerable<ExpressionNode>> SimpleListSeparatorWithItem =
            from separator in SimpleListSeparator.Once()
            from item in SimpleListItem.Once()
            select separator.Concat<ExpressionNode>(item);

        /// <summary>
        ///     Parse a generic MSBuild list, delimited by semicolons.
        /// </summary>
        public static readonly Parser<SimpleList> SimpleList = Parse.Positioned(
            from leadingSeparator in SimpleListLeadingSeparator.Optional()
            from firstItem in SimpleListItem.Once<ExpressionNode>()
            from remainingItems in SimpleListSeparatorWithItem.Many()
            let items =
                leadingSeparator.ToFlattenedSequenceIfDefined()
                    .Concat(firstItem)
                    .Concat(
                        remainingItems.Flatten()
                    )
            select new SimpleList
            {
                Children = ImmutableList.CreateRange(items)
            }
        );

        /// <summary>
        ///     Parse a symbol in an MSBuild expression.
        /// </summary>
        public static readonly Parser<SymbolExpression> Symbol = Parse.Positioned(
            from identifier in Tokens.Identifier
            select new SymbolExpression
            {
                Name = identifier
            }
        );

        /// <summary>
        ///     Parse an equality operator.
        /// </summary>
        public static Parser<ComparisonKind> EqualityOperator =
            from equalityOperator in Tokens.EqualityOperator
            select ComparisonKind.Equality;

        /// <summary>
        ///     Parse an inequality operator.
        /// </summary>
        public static Parser<ComparisonKind> InequalityOperator =
            from equalityOperator in Tokens.InequalityOperator
            select ComparisonKind.Inequality;

        /// <summary>
        ///     Parse a comparison operator.
        /// </summary>
        public static Parser<ComparisonKind> ComparisonOperator = EqualityOperator.Or(InequalityOperator);

        /// <summary>
        ///     Parse an MSBuild comparison expression.
        /// </summary>
        public static readonly Parser<ComparisonExpression> Comparison = Parse.Positioned(
            from leftOperand in Symbol
            from leftWhitespace in Parse.WhiteSpace.Many()
            from comparisonKind in ComparisonOperator
            from rightWhitespace in Parse.WhiteSpace.Many()
            from rightOperand in Symbol
            select new ComparisonExpression
            {
                ComparisonKind = comparisonKind,
                Left = leftOperand,
                Right = rightOperand
            }
        );

        /// <summary>
        ///     Create sequence containing the item.
        /// </summary>
        /// <typeparam name="T">
        ///     The item type.
        /// </typeparam>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <returns>
        ///     A single-element sequence containing the item.
        /// </returns>
        static IEnumerable<T> ToSequence<T>(this T item)
        {
            yield return item;
        }

        /// <summary>
        ///     Create sequence that contains the optional item if it is defined.
        /// </summary>
        /// <typeparam name="T">
        ///     The item type.
        /// </typeparam>
        /// <param name="optionalItem">
        ///     The optional item.
        /// </param>
        /// <returns>
        ///     If <see cref="IOption{T}.IsDefined"/> is <c>true</c>, a single-element sequence containing the item; otherwise, <c>false</c>.
        /// </returns>
        static IEnumerable<T> ToSequenceIfDefined<T>(this IOption<T> optionalItem)
        {
            if (optionalItem == null)
                throw new ArgumentNullException(nameof(optionalItem));

            if (optionalItem.IsDefined)
                yield return optionalItem.Get();
        }

        /// <summary>
        ///     Create flattened sequence that contains the optional items if they defined.
        /// </summary>
        /// <typeparam name="T">
        ///     The item type.
        /// </typeparam>
        /// <param name="optionalItems">
        ///     The optional items.
        /// </param>
        /// <returns>
        ///     If <see cref="IOption{T}.IsDefined"/> is <c>true</c>, a single-element sequence containing the item; otherwise, an empty sequence.
        /// </returns>
        static IEnumerable<T> ToFlattenedSequenceIfDefined<T>(this IOption<IEnumerable<T>> optionalItems)
        {
            if (optionalItems == null)
                throw new ArgumentNullException(nameof(optionalItems));

            if (!optionalItems.IsDefined)
                yield break;

            foreach (T item in optionalItems.Get())
                yield return item;
        }

        /// <summary>
        ///     Create a sequence that contains the optional item or a default value.
        /// </summary>
        /// <typeparam name="T">
        ///     The item type.
        /// </typeparam>
        /// <param name="optionalItem">
        ///     The optional item.
        /// </param>
        /// <param name="valueIfNotDefined">
        ///     The <typeparamref name="T"/> to use if <paramref name="optionalItem"/> is not defined.
        /// </param>
        /// <returns>
        ///     If <see cref="IOption{T}.IsDefined"/> is <c>true</c>, a single-element sequence containing the item; otherwise, a sequence containing <paramref name="valueIfNotDefined"/>.
        /// </returns>
        static IEnumerable<T> ToSequenceOrElse<T>(this IOption<T> optionalItem, T valueIfNotDefined)
        {
            yield return optionalItem.GetOrElse(valueIfNotDefined);
        }
    }
}
