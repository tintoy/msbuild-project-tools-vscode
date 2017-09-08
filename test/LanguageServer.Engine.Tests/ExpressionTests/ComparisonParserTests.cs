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
    public class ComparisonParserTests
    {
        /// <summary>
        ///     Create a new comparison expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public ComparisonParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the ComparisonExpression parser can successfully parse the specified input.
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
        [InlineData("ABC==XYZ",   ComparisonKind.Equality, "ABC", "XYZ")]
        [InlineData("ABC == XYZ", ComparisonKind.Equality, "ABC", "XYZ")]
        [InlineData("ABC!=XYZ",   ComparisonKind.Inequality, "ABC", "XYZ")]
        [InlineData("ABC != XYZ", ComparisonKind.Inequality, "ABC", "XYZ")]
        [Theory(DisplayName = "ComparisonExpression parser succeeds with symbols ")]
        public void ParseComparisonExpression_Symbols_Success(string input, ComparisonKind expectedComparisonKind, string expectedLeftSymbol, string expectedRightSymbol)
        {
            AssertParser.SucceedsWith(Parsers.Comparison, input, actualComparison =>
            {
                Assert.Equal(expectedComparisonKind, actualComparison.ComparisonKind);

                Assert.NotNull(actualComparison.Left);
                SymbolExpression left = Assert.IsType<SymbolExpression>(actualComparison.Left);
                Assert.Equal(left.Name, expectedLeftSymbol);

                Assert.NotNull(actualComparison.Right);
                SymbolExpression right = Assert.IsType<SymbolExpression>(actualComparison.Right);
                Assert.Equal(right.Name, expectedRightSymbol);
            });
        }
    }
}
