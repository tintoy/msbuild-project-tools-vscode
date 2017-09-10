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
        [InlineData("ABC",                        ExpressionKind.Symbol      )]
        [InlineData("'ABC'",                      ExpressionKind.QuotedString)]
        [InlineData("$(ABC)",                     ExpressionKind.Evaluate  )]
        [InlineData("Not ABC",                    ExpressionKind.Logical     )]
        [InlineData("(Not ABC)",                  ExpressionKind.Logical     )]
        [InlineData("ABC And DEF",                ExpressionKind.Logical     )]
        [InlineData("((Not ABC))",                ExpressionKind.Logical     )]
        [InlineData("'ABC' != 'DEF'",             ExpressionKind.Compare  )]
        [InlineData("ABC And (Not DEF)",          ExpressionKind.Logical     )]
        [InlineData("('ABC' != 'DEF')",           ExpressionKind.Compare  )]
        [InlineData("(('ABC' != 'DEF'))",         ExpressionKind.Compare  )]
        [InlineData("(Not ('ABC' != 'DEF'))",     ExpressionKind.Logical     )]
        [InlineData("(Not (('ABC' != 'DEF')))",   ExpressionKind.Logical     )]
        [InlineData("ABC And (Not (DEF Or GHI))", ExpressionKind.Logical     )]
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
    }
}
