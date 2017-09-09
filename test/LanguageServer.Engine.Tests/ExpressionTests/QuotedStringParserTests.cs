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
    ///     Tests for parsing of MSBuild quoted-string expressions.
    /// </summary>
    public class QuotedStringParserTests
    {
        /// <summary>
        ///     Create a new quoted-string expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public QuotedStringParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the QuotedString parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedContent">
        ///     The expected string content.
        /// </param>
        [InlineData("'ABC'"  , "ABC"  )]
        [InlineData("'ABC '" , "ABC " )]
        [InlineData("' ABC'" , " ABC" )]
        [InlineData("' ABC '", " ABC ")]
        [Theory(DisplayName = "QuotedString parser succeeds for simple string ")]
        public void Parse_Success(string input, string expectedContent)
        {
            AssertParser.SucceedsWith(Parsers.QuotedString, input, actualQuotedString =>
            {
                Assert.Equal(expectedContent, actualQuotedString.StringContent);
            });
        }

        /// <summary>
        ///     Verify that the QuotedString parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("ABC"  )]
        [InlineData("ABC " )]
        [InlineData(" ABC" )]
        [InlineData(" ABC ")]
        [Theory(DisplayName = "QuotedString parser fails for unquoted string ")]
        public void Parse_Unquoted_Failure(string input)
        {
            AssertParser.Fails(Parsers.QuotedString, input);
        }

        /// <summary>
        ///     Verify that the QuotedString parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("'ABC")]
        [InlineData("AB'C ")]
        [InlineData(" ABC'")]
        [InlineData(" ABC' ")]
        [Theory(DisplayName = "QuotedString parser fails for string without closing quote ")]
        public void Parse_Without_ClosingQuote_Failure(string input)
        {
            AssertParser.Fails(Parsers.QuotedString, input);
        }

        /// <summary>
        ///     Verify that the QuotedString parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedSymbolName">
        ///     The expected string content.
        /// </param>
        [InlineData("'$(ABC)'", "ABC")]
        [InlineData("'ABC$(DEF)'", "DEF")]
        [InlineData("'ABC $(DEF)'", "DEF")]
        [InlineData("'ABC $(DEF) '", "DEF")]
        [InlineData("'$(ABC)DEF'", "ABC")]
        [InlineData("'$(ABC) DEF'", "ABC")]
        [Theory(DisplayName = "QuotedString parser succeeds for string with evaluated symbol ")]
        public void Parse_Eval_Symbol_Success(string input, string expectedSymbolName)
        {
            AssertParser.SucceedsWith(Parsers.QuotedString, input, actualQuotedString =>
            {
                Evaluation evaluation = actualQuotedString.Evaluations.FirstOrDefault();
                Assert.NotNull(evaluation);

                SymbolExpression symbol = evaluation.Children.OfType<SymbolExpression>().FirstOrDefault();
                Assert.NotNull(symbol);

                Assert.Equal(expectedSymbolName, symbol.Name);
            });
        }
    }
}
