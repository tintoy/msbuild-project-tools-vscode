using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild evaluation expressions.
    /// </summary>
    public class EvaluationParserTests
    {
        /// <summary>
        ///     Create a new evaluation expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public EvaluationParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the Evaluation parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedSymbolName">
        ///     The expected symbol name.
        /// </param>
        [InlineData("$(Foo)",   "Foo")]
        [InlineData("$( Foo )", "Foo")]
        [InlineData("$( Foo)",  "Foo")]
        [InlineData("$(Foo )",  "Foo")]
        [Theory(DisplayName = "Evaluation parser succeeds with symbol ")]
        public void Parse_Symbol_Success(string input, string expectedSymbolName)
        {
            AssertParser.SucceedsWith(Parsers.Evaluation, input, actualEvaluation =>
            {
                Assert.Equal(1, actualEvaluation.Children.Count);

                Symbol actualSymbol = Assert.IsType<Symbol>(actualEvaluation.Children[0]);
                Assert.Equal(expectedSymbolName, actualSymbol.Name);
            });
        }

        /// <summary>
        ///     Verify that the Evaluation parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedFunctionName">
        ///     The expected function name.
        /// </param>
        [InlineData("$( Foo() )",                "Foo")]
        [InlineData("$( Foo('Bar') )",           "Foo")]
        [InlineData("$(Foo.Bar('Baz'))",         "Bar")]
        [InlineData("$([Foo.Bar]::Baz('Bonk'))", "Baz")]
        [Theory(DisplayName = "Evaluation parser succeeds with function-call ")]
        public void Parse_FunctionCall_Success(string input, string expectedFunctionName)
        {
            AssertParser.SucceedsWith(Parsers.Evaluation, input, actualEvaluation =>
            {
                Assert.Equal(1, actualEvaluation.Children.Count);

                FunctionCall actualFunctionCall = Assert.IsType<FunctionCall>(actualEvaluation.Children[0]);
                Assert.Equal(expectedFunctionName, actualFunctionCall.Name);
            });
        }

        /// <summary>
        ///     Verify that the EvaluationExpression parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("$(1Foo)")]
        [InlineData("$(Foo.Bar)")]
        [Theory(DisplayName = "Evaluation parser fails ")]
        public void Parse_Symbol_Failure(string input)
        {
            AssertParser.Fails(Parsers.Evaluation, input);
        }
    }
}
