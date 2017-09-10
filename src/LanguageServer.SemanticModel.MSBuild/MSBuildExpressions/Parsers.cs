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
        ///     Parsers for simple MSBuild lists.
        /// </summary>
        public static class SimpleLists
        {
            /// <summary>
            ///     Parse a simple MSBuild list item.
            /// </summary>
            public static readonly Parser<SimpleListItem> Item = Parse.Positioned(
                from item in Tokens.ListChar.Many().Text()
                select new SimpleListItem
                {
                    Value = item
                }
            );

            /// <summary>
            ///     Parse a simple MSBuild list separator.
            /// </summary>
            public static readonly Parser<ListSeparator> Separator = Parse.Positioned(
                from leading in Parse.WhiteSpace.Many().Text()
                from separator in Tokens.Semicolon
                from trailing in Parse.WhiteSpace.Many().Text()
                select new ListSeparator
                {
                    SeparatorOffset = leading.Length
                }
            );

            /// <summary>
            ///     Parse a simple MSBuild list's leading item separator.
            /// </summary>
            public static readonly Parser<IEnumerable<ExpressionNode>> LeadingSeparator =
                from separator in Separator.Once()
                let leadingEmptyItem = new SimpleListItem
                {
                    Value = String.Empty
                }
                select leadingEmptyItem.ToSequence().Concat<ExpressionNode>(separator);

            /// <summary>
            ///     Parse a simple MSBuild list item separator, optionally followed by a simple list item.
            /// </summary>
            public static readonly Parser<IEnumerable<ExpressionNode>> SeparatorWithItem =
                from separator in Separator.Once()
                from item in Item.Once()
                select separator.Concat<ExpressionNode>(item);

            /// <summary>
            ///     Parse a simple MSBuild list, delimited by semicolons.
            /// </summary>
            public static readonly Parser<SimpleList> List = Parse.Positioned(
                from leadingSeparator in LeadingSeparator.Optional()
                from firstItem in Item.Once<ExpressionNode>()
                from remainingItems in SeparatorWithItem.Many()
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
        }

        /// <summary>
        ///     Parsers for MSBuild list expressions.
        /// </summary>
        public static class Lists
        {
            /// <summary>
            ///     Parse an MSBuild list separator.
            /// </summary>
            public static readonly Parser<ListSeparator> Separator = Parse.Positioned(
                from leading in Parse.WhiteSpace.Many().Text()
                from separator in Tokens.Semicolon
                from trailing in Parse.WhiteSpace.Many().Text()
                select new ListSeparator
                {
                    SeparatorOffset = leading.Length
                }
            );

            /// <summary>
            ///     Parse an MSBuild list's leading item separator.
            /// </summary>
            public static readonly Parser<IEnumerable<ExpressionNode>> LeadingSeparator =
                from separator in Separator.Once()
                let leadingEmptyItem = new EmptyListItem()
                select leadingEmptyItem.ToSequence().Concat<ExpressionNode>(separator);

            /// <summary>
            ///     Parse an MSBuild list item separator, optionally followed by a  list item.
            /// </summary>
            public static readonly Parser<IEnumerable<ExpressionNode>> SeparatorWithItem =
                from separator in Separator.Once()
                from item in Expression.Once()
                select separator.Concat(item);

            /// <summary>
            ///     Parse an MSBuild list, delimited by semicolons.
            /// </summary>
            public static readonly Parser<ExpressionList> List = Parse.Positioned(
                from leadingSeparator in LeadingSeparator.Optional()
                from firstItem in Expression.Once()
                from remainingItems in SeparatorWithItem.Many()
                let items =
                    leadingSeparator.ToFlattenedSequenceIfDefined()
                        .Concat(firstItem)
                        .Concat(
                            remainingItems.Flatten()
                        )
                select new ExpressionList
                {
                    Children = ImmutableList.CreateRange(items)
                }
            );
        }

        /// <summary>
        ///     Parse an MSBuild evaluation expression.
        /// </summary>
        public static Parser<Evaluation> Evaluation = Parse.Positioned(
            from evalOpen in Tokens.EvalOpen
            from body in Tokens.Identifier.Token() // .Or()...
            from evalClose in Tokens.EvalClose
            select new Evaluation
            {
                Children = ImmutableList.Create<ExpressionNode>(
                    new SymbolExpression
                    {
                        Name = body
                    }
                )
            }
        );

        /// <summary>
        ///     Parse a run of contiguous characters in a single-quoted string (excluding <see cref="Tokens.Dollar"/> or the closing <see cref="Tokens.SingleQuote"/>).
        /// </summary>
        public static readonly Parser<StringContent> SingleQuotedStringContent =
            from content in Tokens.SingleQuotedStringChar.Many().Text()
            select new StringContent
            {
                Content = content
            };

        /// <summary>
        ///     Parse an MSBuild quoted-string-literal expression.
        /// </summary>
        public static readonly Parser<QuotedStringLiteral> QuotedStringLiteral = Parse.Positioned(
            from leadingQuote in Tokens.SingleQuote
            from content in SingleQuotedStringContent
            from trailingQuote in Tokens.SingleQuote
            select new QuotedStringLiteral
            {
                Children = ImmutableList.Create<ExpressionNode>(content),
                Content = content.Content
            }
        );

        /// <summary>
        ///     Parse an MSBuild quoted-string-literal expression.
        /// </summary>
        public static readonly Parser<QuotedString> QuotedString = Parse.Positioned(
            from leadingQuote in Tokens.SingleQuote
            from contents in
                SingleQuotedStringContent.As<ExpressionNode>()
                    .Or(Evaluation)
                    .Many()
            from trailingQuote in Tokens.SingleQuote
            select new QuotedString
            {
                Children = ImmutableList.CreateRange(contents)
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
        ///     Parse a binary-expression operand.
        /// </summary>
        public static Parser<ExpressionNode> ComparisonOperand = Symbol.As<ExpressionNode>().Or(QuotedString);

        /// <summary>
        ///     Parse an MSBuild comparison expression.
        /// </summary>
        public static readonly Parser<ComparisonExpression> Comparison = Parse.Positioned(
            from leftOperand in ComparisonOperand
            from comparisonKind in ComparisonOperator.Token()
            from rightOperand in ComparisonOperand
            select new ComparisonExpression
            {
                ComparisonKind = comparisonKind,
                Left = leftOperand,
                Right = rightOperand
            }
        );


        /// <summary>
        ///     Parse a logical-AND operator.
        /// </summary>
        public static Parser<LogicalOperatorKind> AndOperator =
            from andOperator in Tokens.AndOperator
            select LogicalOperatorKind.And;

        /// <summary>
        ///     Parse a logical-OR operator.
        /// </summary>
        public static Parser<LogicalOperatorKind> OrOperator =
            from orOperator in Tokens.OrOperator
            select LogicalOperatorKind.Or;

        /// <summary>
        ///     Parse a logical-NOT operator.
        /// </summary>
        public static Parser<LogicalOperatorKind> NotOperator =
            from orOperator in Tokens.NotOperator
            select LogicalOperatorKind.Not;

        /// <summary>
        ///     Parse a logical-expression operand.
        /// </summary>
        /// <remarks>
        ///     TODO: Work out why we can't just put Refs.GroupedExpression.Or(Refs.Expression) here.
        /// </remarks>
        public static Parser<ExpressionNode> LogicalOperand = Refs.GroupedExpression.Or(Comparison).Or(Evaluation).Or(QuotedString).Or(Symbol);

        /// <summary>
        ///     Parse a logical binary expression.
        /// </summary>
        public static readonly Parser<LogicalExpression> LogicalBinary = Parse.Positioned(
            from leftOperand in LogicalOperand
            from operatorKind in AndOperator.Or(OrOperator).Token()
            from rightOperand in LogicalOperand
            select new LogicalExpression
            {
                OperatorKind = operatorKind,
                Children = ImmutableList.Create(leftOperand, rightOperand)
            }
        );

        /// <summary>
        ///     Parse a logical unary expression.
        /// </summary>
        public static readonly Parser<LogicalExpression> LogicalUnary = Parse.Positioned(
            from operatorKind in NotOperator.Token()
            from rightOperand in LogicalOperand
            select new LogicalExpression
            {
                OperatorKind = operatorKind,
                Children = ImmutableList.Create(rightOperand)
            }
        );

        /// <summary>
        ///     Parse a logical expression.
        /// </summary>
        public static readonly Parser<LogicalExpression> Logical = LogicalUnary.Or(LogicalBinary);

        /// <summary>
        ///     Parse an expression.
        /// </summary>
        public static readonly Parser<ExpressionNode> Expression =
            Logical.As<ExpressionNode>()
                .Or(Comparison)
                .Or(QuotedString)
                .Or(Evaluation)
                .Or(Symbol);

        /// <summary>
        ///     A grouped expression (surrounded by parentheses).
        /// </summary>
        public static readonly Parser<ExpressionNode> GroupedExpression = Parse.Positioned(
            from lparen in Tokens.LParen
            from expression in
                GroupedExpression
                    .Or(Expression)
                    .Token()
            from rparen in Tokens.RParen
            select expression
        );

        /// <summary>
        ///     Parse the root of an expression tree.
        /// </summary>
        public static readonly Parser<ExpressionNode> Root = GroupedExpression.Or(Expression).Or(QuotedString).Token();

        /// <summary>
        ///     Late-bound references to parsers.
        /// </summary>
        static class Refs
        {
            /// <summary>
            ///     Late-bound reference to the <see cref="Parsers.Expression"/> parser.
            /// </summary>
            public static readonly Parser<ExpressionNode> Expression = Parse.Ref(() => Parsers.Expression);

            /// <summary>
            ///     Late-bound reference to the <see cref="Parsers.GroupedExpression"/> parser.
            /// </summary>
            public static readonly Parser<ExpressionNode> GroupedExpression = Parse.Ref(() => Parsers.GroupedExpression);
        }

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

        /// <summary>
        ///     Cast the parser result type.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The parser result type.
        /// </typeparam>
        /// <param name="parser">
        ///     The parser.
        /// </param>
        /// <returns>
        ///     The parser, as one for a sub-type of <typeparamref name="TResult"/>.
        /// </returns>
        static Parser<TResult> As<TResult>(this Parser<TResult> parser) => parser;
    }
}
