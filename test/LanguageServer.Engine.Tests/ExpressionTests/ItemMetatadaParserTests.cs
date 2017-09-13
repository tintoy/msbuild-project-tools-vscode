using System;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;
    using Utilities;

    /// <summary>
    ///     Tests for parsing of MSBuild item metadata expressions.
    /// </summary>
    public class ItemMetadataParserTests
    {
        /// <summary>
        ///     Create a new item metadata expression parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public ItemMetadataParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the ItemMetadata parser can successfully parse an unqualified item metadata expression.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedMetadataName">
        ///     The expected metadata name.
        /// </param>
        [InlineData("%()",      ""   )]
        [InlineData("%(Foo)",   "Foo")]
        [InlineData("%( Foo )", "Foo")]
        [InlineData("%( Foo)",  "Foo")]
        [InlineData("%(Foo )",  "Foo")]
        [Theory(DisplayName = "ItemMetadata parser succeeds ")]
        public void Parse_Unqualified_Success(string input, string expectedMetadataName)
        {
            AssertParser.SucceedsWith(Parsers.ItemMetadata, input, actualItemMetadata =>
            {
                actualItemMetadata.PostParse(
                    new TextPositions(input)
                );

                Assert.Equal(expectedMetadataName, actualItemMetadata.Name);
            });
        }

        /// <summary>
        ///     Verify that the ItemMetadata parser can successfully parse a qualified item metadata expression.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectedItemType">
        ///     The expected item type.
        /// </param>
        /// <param name="expectedMetadataName">
        ///     The expected metadata name.
        /// </param>
        [InlineData("%(Foo.Bar)",   "Foo", "Bar")]
        [InlineData("%( Foo.Bar )", "Foo", "Bar")]
        [InlineData("%( Foo .Bar)", "Foo", "Bar")]
        [InlineData("%(Foo.Bar )",  "Foo", "Bar")]
        [InlineData("%(Foo.)",      "Foo",  ""  )]
        [InlineData("%(Foo. )",     "Foo",  " " )]
        [Theory(DisplayName = "ItemMetadata parser succeeds ")]
        public void Parse_Qualified_Success(string input, string expectedItemType, string expectedMetadataName)
        {
            AssertParser.SucceedsWith(Parsers.ItemMetadata, input, actualItemMetadata =>
            {
                Assert.Equal(expectedItemType, actualItemMetadata.ItemType);
                Assert.Equal(expectedMetadataName, actualItemMetadata.Name);
            });
        }

        /// <summary>
        ///     Verify that the ItemMetadata parser cannot successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("%(1Foo)")]
        [Theory(DisplayName = "ItemMetadata parser fails ")]
        public void Parse_Failure(string input)
        {
            AssertParser.Fails(Parsers.ItemMetadata, input);
        }
    }
}
