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
        ///     Verify that the QuotedStringExpression parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedContent">
        ///     The expected string content.
        /// </param>
        [InlineData("'ABC'",   "ABC"  )]
        [InlineData("'ABC '", "ABC "  )]
        [InlineData("' ABC'", " ABC"  )]
        [InlineData("' ABC '", " ABC ")]
        [Theory(DisplayName = "QuotedStringExpression parser succeeds ")]
        public void Parse_Success(string input, string expectedContent)
        {
            AssertParser.SucceedsWith(Parsers.QuotedString, input, actualQuotedString =>
            {
                Assert.Equal(expectedContent, actualQuotedString.Content);
            });
        }
    }
}
