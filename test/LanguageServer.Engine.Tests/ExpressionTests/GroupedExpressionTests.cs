using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild grouped expressions.
    /// </summary>
    public class GroupedExpressionTests
        : ParserTests
    {
        /// <summary>
        ///     Create a new grouped expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public GroupedExpressionTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        /// <summary>
        ///     Verify that the GroupedExpression parser can successfully parse a logical unary expression composed of a comparison between quoted strings.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedExpressionKind">
        ///     The expected expression kind.
        /// </param>
        [InlineData("(Not ABC)", ExpressionKind.Logical)]
        [InlineData("((Not ABC))", ExpressionKind.Logical)]
        [InlineData("('ABC' != 'DEF')", ExpressionKind.Comparison)]
        [InlineData("(('ABC' != 'DEF'))", ExpressionKind.Comparison)]
        [InlineData("(Not ('ABC' != 'DEF'))", ExpressionKind.Logical)]
        [InlineData("(Not (('ABC' != 'DEF')))", ExpressionKind.Logical)]
        [Theory(DisplayName = "GroupedExpression parser succeeds ")]
        public void Parse_Success(string input, ExpressionKind expectedExpressionKind)
        {
            AssertParser.SucceedsWith(Parsers.GroupedExpression, input, actualExpression =>
            {
                Assert.Equal(expectedExpressionKind, actualExpression.Kind);
            });
        }
    }
}
