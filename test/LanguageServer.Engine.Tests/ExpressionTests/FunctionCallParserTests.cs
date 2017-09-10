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
    ///     Tests for parsing of MSBuild function-call expressions.
    /// </summary>
    public class FunctionCallParserTests
        : ParserTests
    {
        /// <summary>
        ///     Create a new function-call expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public FunctionCallParserTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        /// <summary>
        ///     Verify that the FunctionCall parser can successfully parse a global function-call expression with a single argument.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedArgumentKind">
        ///     The expected argument kind.
        /// </param>
        [InlineData("Exists(Foo)",       ExpressionKind.Symbol      )]
        [InlineData("Exists($(Foo))",    ExpressionKind.Evaluate    )]
        [InlineData("Exists('Foo.txt')", ExpressionKind.QuotedString)]
        [Theory(DisplayName = "FunctionCall parser succeeds for global function call with a single argument ")]
        public void Parse_Global_SingleArgument_String_Success(string input, ExpressionKind expectedArgumentKind)
        {
            AssertParser.SucceedsWith(Parsers.FunctionCalls.Global, input, actualFunctionCall =>
            {
                Assert.Equal(FunctionKind.Global, actualFunctionCall.FunctionKind);
                Assert.Collection(actualFunctionCall.Arguments, actualArgument =>
                {
                    Assert.Equal(expectedArgumentKind, actualArgument.Kind);
                });
            });
        }

        /// <summary>
        ///     Verify that the FunctionCall parser can successfully parse an instance method-call expression with a single argument.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedArgumentKind">
        ///     The expected argument kind.
        /// </param>
        [InlineData("Foo.Exists(Bar)",       ExpressionKind.Symbol      )]
        [InlineData("Foo.Exists($(Bar))",    ExpressionKind.Evaluate    )]
        [InlineData("Foo.Exists('Bar.txt')", ExpressionKind.QuotedString)]
        [Theory(DisplayName = "FunctionCall parser succeeds for instance method call with a single argument ")]
        public void Parse_InstanceMethod_SingleArgument_String_Success(string input, ExpressionKind expectedArgumentKind)
        {
            AssertParser.SucceedsWith(Parsers.FunctionCalls.InstanceMethod, input, actualFunctionCall =>
            {
                Assert.Equal(FunctionKind.InstanceMethod, actualFunctionCall.FunctionKind);
                Assert.Collection(actualFunctionCall.Arguments, actualArgument =>
                {
                    Assert.Equal(expectedArgumentKind, actualArgument.Kind);
                });
            });
        }

        /// <summary>
        ///     Verify that the FunctionCall parser can successfully parse a static method-call expression with a single argument.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedArgumentKind">
        ///     The expected argument kind.
        /// </param>
        [InlineData("[Foo]::Exists(Bar)", ExpressionKind.Symbol)]
        [InlineData("[Foo]::Exists($(Bar))", ExpressionKind.Evaluate)]
        [InlineData("[Foo]::Exists('Bar.txt')", ExpressionKind.QuotedString)]
        [InlineData("[Foo.Bar]::Exists(Baz)", ExpressionKind.Symbol)]
        [InlineData("[Foo.Bar]::Exists($(Baz))", ExpressionKind.Evaluate)]
        [InlineData("[Foo.Bar]::Exists('Baz.txt')", ExpressionKind.QuotedString)]
        [Theory(DisplayName = "FunctionCall parser succeeds for static method call with a single argument ")]
        public void Parse_StaticMethod_SingleArgument_String_Success(string input, ExpressionKind expectedArgumentKind)
        {
            AssertParser.SucceedsWith(Parsers.FunctionCalls.StaticMethod, input, actualFunctionCall =>
            {
                Assert.Equal(FunctionKind.StaticMethod, actualFunctionCall.FunctionKind);
                Assert.Collection(actualFunctionCall.Arguments, actualArgument =>
                {
                    Assert.Equal(expectedArgumentKind, actualArgument.Kind);
                });
            });
        }

        /// <summary>
        ///     Verify that the function-call argument list parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The input to parse.
        /// </param>
        /// <param name="expectedArgumentCount">
        ///     The expected number of arguments.
        /// </param>
        [InlineData("()", 0)]
        [InlineData("('a')", 1)]
        [InlineData("('a', 'b')", 2)]
        [Theory(DisplayName = "ArgumentList parser succeeds ")]
        public void ArgumentList_Success(string input, int expectedArgumentCount)
        {
            AssertParser.SucceedsWith(Parsers.FunctionCalls.ArgumentList, input, actualArgumentList =>
            {
                Assert.Equal(expectedArgumentCount, actualArgumentList.Count());
            });
        }
    }
}
