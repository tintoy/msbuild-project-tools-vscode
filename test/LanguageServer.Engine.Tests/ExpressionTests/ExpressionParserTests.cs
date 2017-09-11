using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel;
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
        [InlineData("! ABC",          ExpressionKind.Logical     )]
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

        /// <summary>
        ///     Verify that the Expression parser can successfully parse an expression such that a most-deeply-nested node can be found by index within the source text.
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
        [InlineData("ABC",                                 00, ExpressionKind.Symbol      , null                        )]
        [InlineData("! ABC",                               00, ExpressionKind.Logical     , null                        )]
        [InlineData("! ABC",                               02, ExpressionKind.Symbol      , ExpressionKind.Logical      )]
        [InlineData("ABC And DEF",                         00, ExpressionKind.Symbol      , ExpressionKind.Logical      )]
        [InlineData("ABC And DEF",                         04, ExpressionKind.Logical     , null                        )]
        [InlineData("ABC And DEF",                         08, ExpressionKind.Symbol      , ExpressionKind.Logical      )]
        [InlineData("$(ABC)",                              00, ExpressionKind.Evaluate    , null                        )]
        [InlineData("$(ABC)",                              02, ExpressionKind.Symbol      , ExpressionKind.Evaluate     )]
        [InlineData("'ABC'",                               00, ExpressionKind.QuotedString, null                        )]
        [InlineData("'ABC' != 'DEF'",                      00, ExpressionKind.QuotedString, ExpressionKind.Compare      )]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 01, ExpressionKind.QuotedString, ExpressionKind.Compare      )]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 03, ExpressionKind.Evaluate    , ExpressionKind.QuotedString )]
        [InlineData(" '$(YetAnotherProperty)' == 'true' ", 26, ExpressionKind.Compare     , null                        )]
        [Theory(DisplayName = "Expression parser succeeds for node at position ")]
        public void FindDeepestNode_Success(string input, int absolutePosition, ExpressionKind expectedExpressionKind, ExpressionKind? parentExpressionKind)
        {
            AssertParser.SucceedsWith(Parsers.Expression, input, actualExpression =>
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
