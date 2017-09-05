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
    ///     Tests for parsing of MSBuild list expressions.
    /// </summary>
    public class ListParserTests
    {
        /// <summary>
        ///     Create a new simple list parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public ListParserTests(ITestOutputHelper testOutput)
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
        ///     Verify that the SimpleListItem parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData(""    )]
        [InlineData(" "   )]
        [InlineData("ABC" )]
        [InlineData(" ABC")]
        [InlineData("ABC ")]
        [Theory(DisplayName = "SimpleListItem parser succeeds ")]
        public void ParseSimpleListItem_Success(string input)
        {
            AssertParser.SucceedsWith(Parsers.SimpleListItem, input, actualItem =>
            {
                Assert.Equal(input, actualItem.Value);
            });
        }

        /// <summary>
        ///     Verify that the <see cref="Parsers.SimpleList"/> MSBuild expression parser can parse a simple simple list in an equivalent fashion to <see cref="String.Split(char[])"/>.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData(";;"      )]
        [InlineData("ABC"     )]
        [InlineData(";ABC"    )]
        [InlineData("ABC;"    )]
        [InlineData(";ABC;"   )]
        [InlineData("ABC;DEF" )]
        [InlineData("ABC;DEF;")]
        [Theory(DisplayName = "Parse MSBuild simple list is equivalent to String.Split ")]
        public void SimpleListEquivalentToStringSplit(string input)
        {
            AssertParser.SucceedsWith(Parsers.SimpleList, input, actualList =>
            {
                DumpList(actualList, input);

                string[] expectedValues = input.Split(';');
                Assert.Collection(actualList.Items, HasListItems(expectedValues, (expectedValue, actualItem) =>
                {
                    Assert.Equal(ExpressionKind.ListItem, actualItem.Kind);

                    SimpleListItem actuaListItem = Assert.IsType<SimpleListItem>(actualItem);
                    Assert.Equal(expectedValue, actuaListItem.Value);
                }));

            });
        }

        /// <summary>
        ///     Verify that a parsed simple list can find an item by its absolute position within the source text.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="position">
        ///     The item's position within the source text.
        /// </param>
        /// <param name="expectedItemValue">
        ///     The expected value of the item.
        /// </param>
        [InlineData("ABC",      0, "ABC")]
        [InlineData("ABC;DEF",  3, "ABC")]
        [InlineData("ABC;DEF",  4, "DEF")]
        [InlineData("ABC;;DEF", 4, ""   )]
        [InlineData("ABC;;DEF", 5, "DEF")]
        [Theory(DisplayName = "SimpleList can find item at position ")]
        public void SimpleList_FindItemAtPosition(string input, int position, string expectedItemValue)
        {
            SimpleList list = MSBuildExpression.ParseSimpleList(input);
            DumpList(list, input);

            SimpleListItem actualItem = list.FindItemAt(position);
            Assert.NotNull(actualItem);

            Assert.Equal(expectedItemValue, actualItem.Value);

            string actualInput = input.Substring(
                startIndex: actualItem.AbsoluteStart,
                length: actualItem.AbsoluteEnd - actualItem.AbsoluteStart
            );
            Assert.Equal(expectedItemValue, actualInput);
        }

        /// <summary>
        ///     Generate test actions for the specified generic item values.
        /// </summary>
        /// <param name="expectedValues">
        ///     The values to expect.
        /// </param>
        /// <param name="testActionTemplate">
        ///     A test action action that receives each expected value and its corresponding actual <see cref="ExpressionNode"/>.
        /// </param>
        /// <returns>
        ///     An array of test actions.
        /// </returns>
        Action<ExpressionNode>[] HasListItems(string[] expectedValues, Action<string, ExpressionNode> testActionTemplate)
        {
            if (expectedValues == null)
                throw new ArgumentNullException(nameof(expectedValues));

            if (testActionTemplate == null)
                throw new ArgumentNullException(nameof(testActionTemplate));

            return
                expectedValues.Select<string, Action<ExpressionNode>>(expectedValue =>
                    actual => testActionTemplate(expectedValue, actual)
                )
                .ToArray();
        }

        /// <summary>
        ///     Dump a simple list to the output for the current test.
        /// </summary>
        /// <param name="list">
        ///     The <see cref="SimpleList"/>.
        /// </param>
        /// <param name="input">
        ///     The original (unparsed) input.
        /// </param>
        void DumpList(SimpleList list, string input)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            TestOutput.WriteLine("Input: '{0}'", input);
            TestOutput.WriteLine(
                new String('=', input.Length + 9)
            );

            foreach (ExpressionNode child in list.Children)
            {
                TestOutput.WriteLine("{0} ({1}..{2})",
                    child.Kind,
                    child.AbsoluteStart,
                    child.AbsoluteEnd
                );
                if (child is SimpleListItem actualItem)
                    TestOutput.WriteLine("\tValue = '{0}'", actualItem.Value);
                else if (child is SimpleListSeparator actualSeparator)
                    TestOutput.WriteLine("\tSeparatorOffset = {0}", actualSeparator.SeparatorOffset);
            }
        }
    }
}
