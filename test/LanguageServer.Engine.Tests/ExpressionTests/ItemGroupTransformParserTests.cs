using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;
    using Sprache;

    /// <summary>
    ///     Tests for parsing of MSBuild item group expressions.
    /// </summary>
    public class ItemGroupTransformParserTests
    {
        /// <summary>
        ///     Create a new item group expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public ItemGroupTransformParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the ItemGroupTransform parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedItemType">
        ///     The expected item group name.
        /// </param>
        /// <param name="expectBody">
        ///     Expect the item group transform to have an expression body?
        /// </param>
        /// <param name="expectSeparator">
        ///     Expect the item group transform to have a separator.
        /// </param>
        [InlineData("@(Foo->)",         "Foo", false, false)]
        [InlineData("@(Foo->'')",       "Foo", true,  false)]
        [InlineData("@(Foo ->'')",      "Foo", true,  false)]
        [InlineData("@(Foo -> '')",     "Foo", true,  false)]
        [InlineData("@(Foo->'','')",    "Foo", true,  true )]
        [InlineData("@(Foo->'', '')",   "Foo", true,  true )]
        [InlineData("@(Foo->'$(Bar)')", "Foo", true,  false)]
        [InlineData("@(Foo->'%(Bar)')", "Foo", true,  false)]
        [Theory(DisplayName = "ItemGroupTransform parser succeeds ")]
        public void Parse_Success(string input, string expectedItemType, bool expectBody, bool expectSeparator)
        {
            AssertParser.SucceedsWith(Parsers.ItemGroupTransform, input, actualItemGroupTransform =>
            {
                Assert.Equal(expectedItemType, actualItemGroupTransform.Name);

                if (expectBody)
                    Assert.True(actualItemGroupTransform.HasBody, "HasBody");
                else
                    Assert.False(actualItemGroupTransform.HasBody, "HasBody");

                if (expectSeparator)
                    Assert.True(actualItemGroupTransform.HasSeparator, "HasSeparator");
                else
                    Assert.False(actualItemGroupTransform.HasSeparator, "HasSeparator");
            });
        }

        /// <summary>
        ///     Verify that the ItemGroupTransform parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("@(Foo)")]
        [InlineData("@(1Foo)")]
        [InlineData("@(Foo.Bar)")]
        [Theory(DisplayName = "ItemGroupTransform parser fails ")]
        public void Parse_Failure(string input)
        {
            AssertParser.Fails(Parsers.ItemGroupTransform, input);
        }
    }
}
