using Sprache;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel;
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild comparison expressions.
    /// </summary>
    public class RootParserTests
    {
        /// <summary>
        ///     Create a new comparison Root parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public RootParserTests(ITestOutputHelper testOutput)
        {
            if (testOutput == null)
                throw new ArgumentNullException(nameof(testOutput));

            TestOutput = testOutput;
        }

        /// <summary>
        ///     Output for the current test.
        /// </summary>
        ITestOutputHelper TestOutput { get; }

        /// <summary>
        ///     Verify that the Root parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedComparisonKind">
        ///     The expected comparison kind.
        /// </param>
        /// <param name="expectedLeftSymbol">
        ///     The expected name of the left-hand symbol.
        /// </param>
        /// <param name="expectedRightSymbol">
        ///     The expected name of the left-hand symbol.
        /// </param>
        [InlineData("ABC==XYZ", ComparisonKind.Equality, "ABC", "XYZ")]
        [InlineData("ABC == XYZ", ComparisonKind.Equality, "ABC", "XYZ")]
        [InlineData("ABC!=XYZ", ComparisonKind.Inequality, "ABC", "XYZ")]
        [InlineData("ABC != XYZ", ComparisonKind.Inequality, "ABC", "XYZ")]
        [Theory(DisplayName = "Root parser succeeds with symbols ")]
        public void ParseRoot_Comparison_Symbols_Success(string input, ComparisonKind expectedComparisonKind, string expectedLeftSymbol, string expectedRightSymbol)
        {
            AssertParser.SucceedsWith(Parsers.Root, input, actualExpression =>
            {
                Compare actualComparison = Assert.IsType<Compare>(actualExpression);

                Assert.Equal(expectedComparisonKind, actualComparison.ComparisonKind);

                Assert.NotNull(actualComparison.Left);
                Symbol left = Assert.IsType<Symbol>(actualComparison.Left);
                Assert.Equal(left.Name, expectedLeftSymbol);

                Assert.NotNull(actualComparison.Right);
                Symbol right = Assert.IsType<Symbol>(actualComparison.Right);
                Assert.Equal(right.Name, expectedRightSymbol);
            });
        }

        /// <summary>
        ///     Verify that the Root parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedRootExpressionKind">
        ///     The expected kind of the root expression.
        /// </param>
        [InlineData("ABC",                      ExpressionKind.Symbol      )]
        [InlineData("'ABC'",                    ExpressionKind.QuotedString)]
        [InlineData("$(ABC)",                   ExpressionKind.Evaluate    )]
        [InlineData("! ABC",                    ExpressionKind.Logical     )]
        [InlineData("(! ABC)",                  ExpressionKind.Logical     )]
        [InlineData("ABC And DEF",              ExpressionKind.Logical     )]
        [InlineData("((! ABC))",                ExpressionKind.Logical     )]
        [InlineData("'ABC' != 'DEF'",           ExpressionKind.Compare     )]
        [InlineData("ABC And (!DEF)",           ExpressionKind.Logical     )]
        [InlineData("('ABC' != 'DEF')",         ExpressionKind.Compare     )]
        [InlineData("(('ABC' != 'DEF'))",       ExpressionKind.Compare     )]
        [InlineData("(! ('ABC' != 'DEF'))",     ExpressionKind.Logical     )]
        [InlineData("(! (('ABC' != 'DEF')))",   ExpressionKind.Logical     )]
        [InlineData("ABC And (! (DEF Or GHI))", ExpressionKind.Logical     )]
        [Theory(DisplayName = "Root parser succeeds with expression kind ")]
        public void ParseRoot_Logical_Success(string input, ExpressionKind expectedRootExpressionKind)
        {
            AssertParser.SucceedsWith(Parsers.Root, input, actualExpression =>
            {
                Assert.Equal(expectedRootExpressionKind, actualExpression.Kind);
            });
        }

        /// <summary>
        ///     Verify that the Root parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedComparisonKind">
        ///     The expected comparison kind.
        /// </param>
        /// <param name="expectedLeftContent">
        ///     The expected content of the left-hand string.
        /// </param>
        /// <param name="expectedRightContent">
        ///     The expected content of the left-hand string.
        /// </param>
        [InlineData("'ABC'=='XYZ'", ComparisonKind.Equality, "ABC", "XYZ")]
        [InlineData("'ABC' == 'XYZ'", ComparisonKind.Equality, "ABC", "XYZ")]
        [InlineData("'ABC'!='XYZ'", ComparisonKind.Inequality, "ABC", "XYZ")]
        [InlineData("'ABC' != 'XYZ'", ComparisonKind.Inequality, "ABC", "XYZ")]
        [Theory(DisplayName = "Root parser succeeds with quoted strings ")]
        public void ParseRoot_QuotedStrings_Success(string input, ComparisonKind expectedComparisonKind, string expectedLeftContent, string expectedRightContent)
        {
            AssertParser.SucceedsWith(Parsers.Root, input, actualExpression =>
            {
                Compare actualComparison = Assert.IsType<Compare>(actualExpression);

                Assert.Equal(expectedComparisonKind, actualComparison.ComparisonKind);

                Assert.NotNull(actualComparison.Left);
                QuotedString left = Assert.IsType<QuotedString>(actualComparison.Left);
                Assert.Equal(expectedLeftContent, left.StringContent);

                Assert.NotNull(actualComparison.Right);
                QuotedString right = Assert.IsType<QuotedString>(actualComparison.Right);
                Assert.Equal(expectedRightContent, right.StringContent);
            });
        }

        /// <summary>
        ///     Verify that the root parser can successfully parse an expression such that a most-deeply-nested node can be found by index within the source text.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="absolutePosition">
        ///     The absolute position, within the source text, of the node to find.
        /// </param>
        /// <param name="expectedExpressionKind">
        ///     The expected expression kind.
        /// </param>
        /// <param name="expectedParentExpressionKind">
        ///     The expected parent expression kind (if any).
        /// </param>
        [InlineData("ABC",                                 00, ExpressionKind.Symbol      , null                       )]
        [InlineData("! ABC",                               00, ExpressionKind.Logical     , null                       )]
        [InlineData("! ABC",                               02, ExpressionKind.Symbol      , ExpressionKind.Logical     )]
        [InlineData("ABC And DEF",                         00, ExpressionKind.Symbol      , ExpressionKind.Logical     )]
        [InlineData("ABC And DEF",                         04, ExpressionKind.Logical     , null                       )]
        [InlineData("ABC And DEF",                         08, ExpressionKind.Symbol      , ExpressionKind.Logical     )]
        [InlineData("$(ABC)",                              00, ExpressionKind.Evaluate    , null                       )]
        [InlineData("$(ABC)",                              02, ExpressionKind.Symbol      , ExpressionKind.Evaluate    )]
        [InlineData("'ABC'",                               00, ExpressionKind.QuotedString, null                       )]
        [InlineData("'ABC' != 'DEF'",                      00, ExpressionKind.QuotedString, ExpressionKind.Compare     )]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 01, ExpressionKind.QuotedString, ExpressionKind.Compare     )]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 02, ExpressionKind.Evaluate,     ExpressionKind.QuotedString)]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 03, ExpressionKind.Evaluate    , ExpressionKind.QuotedString)]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 04, ExpressionKind.Symbol      , ExpressionKind.Evaluate    )]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 26, ExpressionKind.Compare     , null                       )]
        [Theory(DisplayName = "Root parser succeeds for node at position ")]
        public void FindDeepestNode_Success(string input, int absolutePosition, ExpressionKind expectedExpressionKind, ExpressionKind? parentExpressionKind)
        {
            AssertParser.SucceedsWith(Parsers.Root, input, actualExpression =>
            {
                actualExpression.EnsureRelationships();

                ExpressionNode actualNodeAtPosition = actualExpression.FindDeepestNodeAt(absolutePosition);
                Assert.NotNull(actualNodeAtPosition);
                Assert.Equal(expectedExpressionKind, actualNodeAtPosition.Kind);
                Assert.Equal(parentExpressionKind, actualNodeAtPosition.Parent?.Kind);
            });
        }
    }
}
