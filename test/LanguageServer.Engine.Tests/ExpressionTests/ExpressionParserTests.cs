using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild expressions.
    /// </summary>
    public class ExpressionParserTests
        : ParserTests
    {
        /// <summary>
        ///     Create a new expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public ExpressionParserTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        /// <summary>
        ///     Verify that the Expression parser can successfully parse an expression.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedExpressionKind">
        ///     The expected expression kind.
        /// </param>
        [InlineData("ABC",            ExpressionKind.Symbol      )]
        [InlineData("Not ABC",        ExpressionKind.Logical     )]
        [InlineData("ABC And DEF",    ExpressionKind.Logical     )]
        [InlineData("$(ABC)",         ExpressionKind.Evaluate    )]
        [InlineData("'ABC'",          ExpressionKind.QuotedString)]
        [InlineData("'ABC' != 'DEF'", ExpressionKind.Compare     )]
        [Theory(DisplayName = "Expression parser succeeds ")]
        public void Parse_Success(string input, ExpressionKind expectedExpressionKind)
        {
            AssertParser.SucceedsWith(Parsers.Expression, input, actualExpression =>
            {
                Assert.Equal(expectedExpressionKind, actualExpression.Kind);
            });
        }
    }
}
