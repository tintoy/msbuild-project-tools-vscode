using Sprache;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild logical expressions.
    /// </summary>
    public class LogicalParserTests
        : ParserTests
    {
        /// <summary>
        ///     Create a new logical expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public LogicalParserTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        /// <summary>
        ///     Verify that the ComparisonExpression parser can successfully parse a logical binary expression composed of 2 comparisons between quoted strings.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedLeftComparisonKind">
        ///     The expected left-hand comparison kind.
        /// </param>
        /// <param name="expectedRightComparisonKind">
        ///     The expected left-hand comparison kind.
        /// </param>
        /// <param name="expectedLeftLeftString">
        ///     The expected name of the left-hand string in the left-hand comparison.
        /// </param>
        /// <param name="expectedLeftRightString">
        ///     The expected name of the right-hand string in the left-hand comparison.
        /// </param>
        /// <param name="expectedRightLeftString">
        ///     The expected name of the left-hand string in the right-hand comparison.
        /// </param>
        /// <param name="expectedRightRightString">
        ///     The expected name of the right-hand string in the right-hand comparison.
        /// </param>
        [InlineData("'ABC' == 'DEF' And 'GHI' == 'JKL'",
            LogicalOperatorKind.And, ComparisonKind.Equality, ComparisonKind.Equality,
            "ABC", "DEF", "GHI", "JKL"
        )]
        [InlineData("'ABC' != 'DEF' And 'GHI' != 'JKL'",
            LogicalOperatorKind.And, ComparisonKind.Inequality, ComparisonKind.Inequality,
            "ABC", "DEF", "GHI", "JKL"
        )]
        [InlineData("'ABC' == 'DEF' Or 'GHI' != 'JKL'",
            LogicalOperatorKind.Or, ComparisonKind.Equality, ComparisonKind.Inequality,
            "ABC", "DEF", "GHI", "JKL"
        )]
        [InlineData("'ABC'!='DEF'Or'GHI'=='JKL'",
            LogicalOperatorKind.Or, ComparisonKind.Inequality, ComparisonKind.Equality,
            "ABC", "DEF", "GHI", "JKL"
        )]
        [Theory(DisplayName = "LogicalExpression parser succeeds with strings ")]
        public void Parse_Binary_Comparison_QuotedString_Success(string input, LogicalOperatorKind expectedLogicalOperatorKind, ComparisonKind expectedLeftComparisonKind, ComparisonKind expectedRightComparisonKind, string expectedLeftLeftString, string expectedLeftRightString, string expectedRightLeftString, string expectedRightRightString)
        {
            AssertParser.SucceedsWith(Parsers.Logical, input, actualLogical =>
            {
                Assert.Equal(expectedLogicalOperatorKind, actualLogical.OperatorKind);

                // Left-hand comparison.
                ComparisonExpression leftComparison = Assert.IsType<ComparisonExpression>(actualLogical.Left);
                Assert.Equal(expectedLeftComparisonKind, leftComparison.ComparisonKind);

                QuotedString leftLeftString = Assert.IsType<QuotedString>(leftComparison.Left);
                Assert.Equal(expectedLeftLeftString, leftLeftString.StringContent);

                QuotedString leftRightString = Assert.IsType<QuotedString>(leftComparison.Right);
                Assert.Equal(expectedLeftRightString, leftRightString.StringContent);

                // Right-hand comparison.
                ComparisonExpression rightComparison = Assert.IsType<ComparisonExpression>(actualLogical.Right);
                Assert.Equal(expectedRightComparisonKind, rightComparison.ComparisonKind);

                QuotedString rightLeftString = Assert.IsType<QuotedString>(rightComparison.Left);
                Assert.Equal(expectedRightLeftString, rightLeftString.StringContent);

                QuotedString rightRightString = Assert.IsType<QuotedString>(rightComparison.Right);
                Assert.Equal(expectedRightRightString, rightRightString.StringContent);
            });
        }
    }
}
