using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild tokens.
    /// </summary>
    public class TokenParserTests
    {
        /// <summary>
        ///     Create a new token parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public TokenParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the Identifier parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedIdentifierName">
        ///     The expected identifier name.
        /// </param>
        [InlineData("F", "F")]
        [InlineData("Foo", "Foo")]
        [Theory(DisplayName = "Identifier token parser succeeds ")]
        public void Parse_Success(string input, string expectedIdentifierName)
        {
            AssertParser.SucceedsWith(Tokens.Identifier, input, actualToken =>
            {
                Assert.Equal(expectedIdentifierName, actualToken);
            });
        }

        /// <summary>
        ///     Verify that the Identifier parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("1")]
        [InlineData("1Foo")]
        [Theory(DisplayName = "Identifier token parser fails ")]
        public void Parse_Failure(string input)
        {
            AssertParser.Fails(Tokens.Identifier, input);
        }
    }
}
