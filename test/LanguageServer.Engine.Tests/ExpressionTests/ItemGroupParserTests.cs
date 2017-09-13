using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild item group expressions.
    /// </summary>
    public class ItemGroupParserTests
    {
        /// <summary>
        ///     Create a new item group expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public ItemGroupParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the ItemGroup parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedItemGroupName">
        ///     The expected symbol name.
        /// </param>
        [InlineData("@()",      ""   )]
        [InlineData("@(Foo)",   "Foo")]
        [InlineData("@( Foo )", "Foo")]
        [InlineData("@( Foo)",  "Foo")]
        [InlineData("@(Foo )",  "Foo")]
        [Theory(DisplayName = "ItemGroup parser succeeds ")]
        public void Parse_Success(string input, string expectedItemGroupName)
        {
            AssertParser.SucceedsWith(Parsers.ItemGroup, input, actualItemGroup =>
            {
                Assert.Equal(expectedItemGroupName, actualItemGroup.Name);
            });
        }

        /// <summary>
        ///     Verify that the ItemGroup parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("@(1Foo)")]
        [InlineData("@(Foo.Bar)")]
        [Theory(DisplayName = "ItemGroup parser fails ")]
        public void Parse_Failure(string input)
        {
            AssertParser.Fails(Parsers.ItemGroup, input);
        }
    }
}
