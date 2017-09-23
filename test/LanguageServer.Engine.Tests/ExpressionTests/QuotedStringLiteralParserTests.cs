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
    ///     Tests for parsing of MSBuild quoted-string literal expressions.
    /// </summary>
    public class QuotedStringLiteralParserTests
    {
        /// <summary>
        ///     Create a new quoted-string literal expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public QuotedStringLiteralParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the QuotedStringLiteralExpression parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedContent">
        ///     The expected string content.
        /// </param>
        [InlineData("''",      ""     )]
        [InlineData("'ABC'",   "ABC"  )]
        [InlineData("'ABC '", "ABC "  )]
        [InlineData("' ABC'", " ABC"  )]
        [InlineData("' ABC '", " ABC ")]
        [Theory(DisplayName = "QuotedStringLiteral parser succeeds ")]
        public void Parse_Success(string input, string expectedContent)
        {
            AssertParser.SucceedsWith(Parsers.QuotedStringLiteral, input, actualQuotedStringLiteral =>
            {
                Assert.Equal(expectedContent, actualQuotedStringLiteral.Content);
            });
        }

        /// <summary>
        ///     Verify that the QuotedStringLiteral parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("ABC")]
        [InlineData("ABC ")]
        [InlineData(" ABC")]
        [InlineData(" ABC ")]
        [Theory(DisplayName = "QuotedStringLiteral parser fails for unquoted string ")]
        public void Parse_Unquoted_Failure(string input)
        {
            AssertParser.Fails(Parsers.QuotedStringLiteral, input);
        }

        /// <summary>
        ///     Verify that the QuotedStringLiteral parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("'ABC")]
        [InlineData("AB'C ")]
        [InlineData(" ABC'")]
        [InlineData(" ABC' ")]
        [Theory(DisplayName = "QuotedStringLiteral parser fails for string without closing quote ")]
        public void Parse_Without_ClosingQuote_Failure(string input)
        {
            AssertParser.Fails(Parsers.QuotedStringLiteral, input);
        }
    }
}
