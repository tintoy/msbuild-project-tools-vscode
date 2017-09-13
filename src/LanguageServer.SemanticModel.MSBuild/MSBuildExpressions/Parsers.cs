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
    /// <remarks>
    ///     AF: Be careful when combining Parse.Ref with Parse.Or; you want to use Parse.Ref(() => X.Or(Y)) not Parse.Ref(() => X).Or(Parse.Ref(() => Y)).
    /// </remarks>
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
                from leadingEmptyItem in
                    Parse.Return(new SimpleListItem
                    {
                        Value = String.Empty
                    })
                    .Positioned()
                from separator in Separator.Once()
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
                    leadingSeparator.ToSequenceIfDefined()
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
                    leadingSeparator.ToSequenceIfDefined()
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
        ///     Parsers for MSBuild function-call expressions.
        /// </summary>
        public static class FunctionCalls
        {
            /// <summary>
            ///     Parse a function argument.
            /// </summary>
            public static readonly Parser<ExpressionNode> Argument =
                QuotedString.As<ExpressionNode>()
                    .Or(Symbol)
                    .Or(Evaluation)
                    .Or(ItemGroup)
                    .Or(ItemMetadata)
                    .Named("function argument");

            /// <summary>
            ///     Parse a function argument list.
            /// </summary>
            public static readonly Parser<IEnumerable<ExpressionNode>> ArgumentList =
                from openingParenthesis in Tokens.LParen.Token().Named("open function argument list")
                from functionArguments in Argument.Token().DelimitedBy(Tokens.Comma).Named("function argument list").Optional()
                from closingParenthesis in Tokens.RParen.Token().Named("close function argument list")
                select functionArguments.ToSequenceIfDefined();

            /// <summary>
            ///     Parse a global function-call.
            /// </summary>
            public static readonly Parser<FunctionCall> Global = Parse.Positioned(
                from functionName in Symbol.Named("function name")
                from functionArguments in ArgumentList.Named("function argument list")
                select new FunctionCall
                {
                    FunctionKind = FunctionKind.Global,
                    Name = functionName.Name,
                    Children = ImmutableList.CreateRange(functionArguments)
                }
            ).Named("global function call");

            /// <summary>
            ///     Parse an instance method function-call.
            /// </summary>
            public static readonly Parser<FunctionCall> InstanceMethod = Parse.Positioned(
                from target in Symbol.Token().Once().Named("function call method target")
                from period in Tokens.Period.Token()
                from methodName in Symbol.Token().Named("function name")
                from functionArguments in ArgumentList.Named("function argument list")
                select new FunctionCall
                {
                    FunctionKind = FunctionKind.InstanceMethod,
                    Name = methodName.Name,
                    Children = ImmutableList.CreateRange(
                        target.Concat(functionArguments)
                    )
                }
            ).Named("instance method call");

            /// <summary>
            ///     Parse a static method function-call.
            /// </summary>
            public static readonly Parser<FunctionCall> StaticMethod = Parse.Positioned(
                from target in TypeRef.Token().Once().Named("function call method target")
                from doubleColon in Tokens.Colon.Repeat(2)
                from methodName in Symbol.Token().Named("function name")
                from functionArguments in ArgumentList.Named("function argument list")
                select new FunctionCall
                {
                    FunctionKind = FunctionKind.StaticMethod,
                    Name = methodName.Name,
                    Children = ImmutableList.CreateRange(
                        target.Concat(functionArguments)
                    )
                }
            ).Named("instance method call");

            /// <summary>
            ///     Parse any kind of function-call expression.
            /// </summary>
            public static readonly Parser<FunctionCall> Any = StaticMethod.Or(InstanceMethod).Or(Global);
        }

        /// <summary>
        ///     Parse the body of an evaluation expression.
        /// </summary>
        public static Parser<ExpressionNode> EvaluationBody = Parse.Ref(() =>
            FunctionCalls.Any.As<ExpressionNode>()
                .Or(Symbol)
        );

        /// <summary>
        ///     Parse an MSBuild evaluation expression.
        /// </summary>
        /// <remarks>
        ///     The symbol between the parentheses is optional so we can still provide completions for "$()".
        /// </remarks>
        public static Parser<Evaluate> Evaluation = Parse.Positioned(
            from evalOpen in Tokens.EvalOpen.Named("open evaluation")
            from body in EvaluationBody.Token().Optional().Named("evaluation body")
            from evalClose in Tokens.EvalClose.Named("close evaluation")
            select new Evaluate
            {
                Children =
                    body.IsDefined
                        ? ImmutableList.Create(body.Get())
                        : ImmutableList<ExpressionNode>.Empty
            }
        ).Named("evaluation");

        /// <summary>
        ///     Parse an MSBuild item group expression.
        /// </summary>
        /// <remarks>
        ///     The symbol between the parentheses is optional so we can still provide completions for "@()".
        /// </remarks>
        public static Parser<ItemGroup> ItemGroup = Parse.Positioned(
            from itemGroupOpen in Tokens.ItemGroupOpen.Named("open item group")
            from name in Symbol.Or(EmptySymbol).Token().Named("item group name")
            from itemGroupClose in Tokens.ItemGroupClose.Named("close item group")
            select new ItemGroup
            {
                Children = ImmutableList.Create<ExpressionNode>(name)
            }
        ).Named("item group");

        /// <summary>
        ///     Parse an MSBuild item metadata expression, "%(ItemType.MetadataName") or "%(MetadataName)".
        /// </summary>
        /// <remarks>
        ///     The symbols between the parentheses are optional so we can still provide completions for "%()".
        ///     
        ///     We model the item type and metadata name as 2 separate symbols because we have scenarios where we want to address them separately.
        /// </remarks>
        public static Parser<ItemMetadata> ItemMetadata = Parse.Positioned(
            from metadataOpen in Tokens.ItemMetadataOpen.Named("open item metadata")
            from itemTypeOrMetadataName in Symbol.Token().Optional().Named("item type or metadata name")
            from separator in Tokens.Period.Token().Optional().Named("item type separator")
            from metadataName in Symbol.Token().Optional().Named("item metadata name")
            from metadataClose in Tokens.ItemMetadataClose.Named("close item metadata")
            select new ItemMetadata
            {
                Children = ImmutableList.CreateRange<ExpressionNode>(
                    itemTypeOrMetadataName.ToSequenceIfDefined().Concat(
                        metadataName.ToSequenceIfDefined()
                    )
                )
            }
        ).Named("item metadata");

        /// <summary>
        ///     Parse a run of contiguous characters in a single-quoted string (excluding <see cref="Tokens.Dollar"/> or the closing <see cref="Tokens.SingleQuote"/>).
        /// </summary>
        public static readonly Parser<StringContent> SingleQuotedStringContent =
            from content in Tokens.SingleQuotedStringChar.Many().Text().Named("string content")
            select new StringContent
            {
                Content = content
            };

        /// <summary>
        ///     Parse an MSBuild quoted-string-literal expression.
        /// </summary>
        public static readonly Parser<QuotedStringLiteral> QuotedStringLiteral = Parse.Positioned(
            from leadingQuote in Tokens.SingleQuote.Named("open quoted string literal")
            from content in SingleQuotedStringContent.Named("quoted string literal content")
            from trailingQuote in Tokens.SingleQuote.Named("close quoted string literal")
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
            from leadingQuote in Tokens.SingleQuote.Named("open quoted string")
            from contents in
                SingleQuotedStringContent.As<ExpressionNode>()
                    .Or(Evaluation)
                    .Or(ItemGroup)
                    .Or(ItemMetadata)
                    .Many()
                    .Named("quoted string content")
            from trailingQuote in Tokens.SingleQuote.Named("close quoted string")
            select new QuotedString
            {
                Children = ImmutableList.CreateRange(contents)
            }
        ).Named("quoted string");

        /// <summary>
        ///     Parse a symbol in an MSBuild expression.
        /// </summary>
        public static readonly Parser<Symbol> Symbol = Parse.Positioned(
            from identifier in Tokens.Identifier.Named("identifier")
            select new Symbol
            {
                Name = identifier
            }
        ).Named("symbol");

        /// <summary>
        ///     Parse an empty symbol in an MSBuild expression.
        /// </summary>
        public static readonly Parser<Symbol> EmptySymbol =
            Parse.Return(new Symbol
            {
                Name = String.Empty
            })
            .Positioned()
            .Named("empty symbol");

        /// <summary>
        ///     Parse a symbol in an MSBuild expression.
        /// </summary>
        public static readonly Parser<Symbol> QualifiedSymbol = Parse.Positioned(
            from identifiers in Tokens.Identifier.DelimitedBy(Tokens.Period).Array().Named("identifiers")
            
            select new Symbol
            {
                Name = identifiers[identifiers.Length - 1],
                Namespace = String.Join(".", identifiers.Take(identifiers.Length - 1))
            }
        ).Named("qualified symbol");

        /// <summary>
        ///     Parse a type-reference expression.
        /// </summary>
        public static readonly Parser<Symbol> TypeRef = Parse.Positioned(
            from openType in Tokens.LBracket.Token().Named("open TypeRef name")
            from type in QualifiedSymbol.Token().Named("TypeRef name")
            from closeType in Tokens.RBracket.Token().Named("close TypeRef name")
            select type
        ).Named("type reference");

        /// <summary>
        ///     Parse an equality operator.
        /// </summary>
        public static Parser<ComparisonKind> EqualityOperator =
            from equalityOperator in Tokens.EqualityOperator.Named("equality operator")
            select ComparisonKind.Equality;

        /// <summary>
        ///     Parse an inequality operator.
        /// </summary>
        public static Parser<ComparisonKind> InequalityOperator =
            from equalityOperator in Tokens.InequalityOperator.Named("inequality operator")
            select ComparisonKind.Inequality;

        /// <summary>
        ///     Parse a comparison operator.
        /// </summary>
        public static Parser<ComparisonKind> ComparisonOperator = EqualityOperator.Or(InequalityOperator);

        /// <summary>
        ///     Parse a binary-expression operand.
        /// </summary>
        /// <remarks>
        ///     In order to avoid creating a left-recursive grammar, we can't just use Expression here; instead, we have to spell out what's allowed in a comparison operand.
        /// </remarks>
        public static Parser<ExpressionNode> ComparisonOperand = Symbol.As<ExpressionNode>().Or(QuotedString);

        /// <summary>
        ///     Parse an MSBuild comparison expression.
        /// </summary>
        public static readonly Parser<Compare> Comparison = Parse.Positioned(
            from leftOperand in ComparisonOperand.Token().Named("left operand")
            from comparisonKind in ComparisonOperator.Token().Named("comparison operator")
            from rightOperand in ComparisonOperand.Token().Named("right operand")
            select new Compare
            {
                ComparisonKind = comparisonKind,
                Children = ImmutableList.Create(leftOperand, rightOperand)
            }
        );

        /// <summary>
        ///     Parse a logical-AND operator.
        /// </summary>
        public static Parser<LogicalOperatorKind> AndOperator =
            from andOperator in Tokens.AndOperator.Named("logical-AND operator")
            select LogicalOperatorKind.And;

        /// <summary>
        ///     Parse a logical-OR operator.
        /// </summary>
        public static Parser<LogicalOperatorKind> OrOperator =
            from orOperator in Tokens.OrOperator.Named("logical-OR operator")
            select LogicalOperatorKind.Or;

        /// <summary>
        ///     Parse a logical-NOT operator.
        /// </summary>
        public static Parser<LogicalOperatorKind> NotOperator =
            from orOperator in Tokens.NotOperator.Named("logical-NOT operator")
            select LogicalOperatorKind.Not;

        /// <summary>
        ///     Parse a logical-expression operand.
        /// </summary>
        /// <remarks>
        ///     In order to avoid creating a left-recursive grammar, we can't just use Expression here; instead, we have to spell out what's allowed in a logical operand.
        /// </remarks>
        public static Parser<ExpressionNode> LogicalOperand = Parse.Ref(() =>
            GroupedExpression
                .Or(Comparison)
                .Or(Evaluation)
                .Or(ItemGroup)
                .Or(ItemMetadata)
                .Or(QuotedString)
                .Or(Symbol)
        );

        /// <summary>
        ///     Parse a logical binary expression.
        /// </summary>
        public static readonly Parser<LogicalExpression> LogicalBinary = Parse.Positioned(
            from leftOperand in LogicalOperand.Token().Named("left operand")
            from operatorKind in AndOperator.Or(OrOperator).Token().Named("binary operator")
            from rightOperand in LogicalOperand.Token().Named("right operand")
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
            from operatorKind in NotOperator.Token().Named("unary operator")
            from rightOperand in LogicalOperand.Token().Named("right operand")
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
                .Or(ItemGroup)
                .Or(ItemMetadata)
                .Or(Symbol);

        /// <summary>
        ///     A grouped expression (surrounded by parentheses).
        /// </summary>
        public static readonly Parser<ExpressionNode> GroupedExpression = Parse.Positioned(
            from lparen in Tokens.LParen.Named("open sub-expression")
            from expression in
                GroupedExpression
                    .Or(Expression)
                    .Token()
            from rparen in Tokens.RParen.Named("close sub-expression")
            select expression
        );

        // TODO: Implement ExpressionTree (an ExpressionNode representing the root of an expression tree).

        /// <summary>
        ///     Parse the root of an expression tree.
        /// </summary>
        public static readonly Parser<ExpressionTree> Root = Parse.Positioned(
            from expressions in GroupedExpression.Or(Expression).Or(QuotedString).Token().Many()
            select new ExpressionTree
            {
                Children = ImmutableList.CreateRange(expressions)
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
        ///     Create a sequence that contains the optional items if they defined.
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
        static IEnumerable<T> ToSequenceIfDefined<T>(this IOption<IEnumerable<T>> optionalItems)
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
        ///     Create a new sequence that contains all elements in the source sequence except the last element.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The type of element contained in the source sequence.
        /// </typeparam>
        /// <param name="source">
        ///     The source sequence.
        /// </param>
        /// <returns>
        ///     The new sequence.
        /// </returns>
        static IEnumerable<TSource> DropLast<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source is IList<TSource> list)
            {
                for (int index = 0; index < list.Count - 1; index++)
                    yield return list[index];

                yield break;
            }

            using (IEnumerator<TSource> sequence = source.GetEnumerator())
            {
                if (!sequence.MoveNext())
                    yield break;

                TSource previousElement = sequence.Current;
                while (sequence.MoveNext())
                {
                    yield return previousElement;

                    previousElement = sequence.Current;
                }
            }
        }

        /// <summary>
        ///     Convert the parsed sequence to an array.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The parser result type.
        /// </typeparam>
        /// <param name="parser">
        ///     The parser whose result is a sequence of <typeparamref name="TResult"/>.
        /// </param>
        /// <returns>
        ///     A parser whose result type is an array of <typeparamref name="TResult"/>.
        /// </returns>
        static Parser<TResult[]> Array<TResult>(this Parser<IEnumerable<TResult>> parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            return input =>
            {
                IResult<IEnumerable<TResult>> result = parser(input);
                if (result.WasSuccessful)
                    return Result.Success(result.Value.ToArray(), result.Remainder);

                return Result.Failure<TResult[]>(result.Remainder, result.Message, result.Expectations);
            };
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
