using Sprache;
using System;
using System.Collections.Generic;
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
        ///     Create a new generic list parser test-suite.
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
        ///     Verify that the GenericListItem parser can successfully parse the specified input.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("ABC")]
        [InlineData(" ABC")]
        [InlineData("ABC ")]
        [Theory(DisplayName = "GenericListItem parser succeeds ")]
        public void ParseGenericListItem_Success(string input)
        {
            AssertParser.SucceedsWith(Parsers.GenericListItem, input, actualItem =>
            {
                Assert.Equal(input, actualItem.Value);
            });
        }

        /// <summary>
        ///     Verify that the <see cref="Parsers.GenericList"/> MSBuild expression parser can parse a simple generic list in an equivalent fashion to <see cref="String.Split(char[])"/>.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        [InlineData("ABC")]
        [InlineData(";ABC")]
        [InlineData("ABC;")]
        [InlineData(";ABC;")]
        [InlineData("ABC;DEF")]
        [InlineData("ABC;DEF;")]
        [Theory(DisplayName = "Parse MSBuild generic list is equivalent to String.Split ")]
        public void GenericListEquivalentToStringSplit(string input)
        {
            string[] expectedValues = input.Split(';');
            Action<ExpressionNode>[] itemTests = GenericItemTests(expectedValues, (expectedValue, actualItem) =>
            {
                Assert.Equal(ExpressionKind.ListItem, actualItem.Kind);

                GenericListItem actuaListItem = Assert.IsType<GenericListItem>(actualItem);
                Assert.Equal(expectedValue, actuaListItem.Value);
            });

            AssertParser.SucceedsWith(Parsers.GenericList, input, actualList =>
            {
                DumpList(actualList, input);

                Assert.Collection(actualList.Items, itemTests);
            });
        }

        /// <summary>
        ///     Verify that a parsed generic list can find an item by its absolute position within the source text.
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
        [InlineData("ABC", 0, "ABC")]
        [InlineData("ABC;DEF", 3, "ABC")]
        [InlineData("ABC;DEF", 4, "DEF")]
        [Theory(DisplayName = "GenericList can find item at position ")]
        public void GenericList_FindItemAtPosition(string input, int position, string expectedItemValue)
        {
            GenericList list = MSBuildExpression.ParseGenericList(input);
            DumpList(list, input);

            GenericListItem actualItem = list.FindItemAt(position);
            Assert.NotNull(actualItem);

            Assert.Equal(expectedItemValue, actualItem.Value);
        }

        /// <summary>
        ///     Explicitly parse the components of a generic list, and verify that the parse is successful.
        /// </summary>
        /// <param name="input">
        ///     The source text to parse.
        /// </param>
        /// <param name="expectLeadingSeparator">
        ///     Expect a leading separator?
        /// </param>
        /// <param name="expectFirstItem">
        ///     Expect a first item?
        /// </param>
        /// <param name="expectedRemainingItemCount">
        ///     The number of remaining items (if any) to expect.
        /// </param>
        /// <param name="expectTrailingEmptyItem">
        ///     Expect a trailing empty item?
        /// </param>
        /// <remarks>
        ///     This test is mainly used to assist with diagnostic scenarios.
        /// </remarks>
        [InlineData(";ABC", true, true, 0, false)]
        [InlineData("ABC;", false, true, 1, true)]
        [InlineData("ABC;DEF;", false, true, 2, true)]
        [InlineData(";ABC;DEF;", true, true, 2, true)]
        [Theory(DisplayName = "Explicit parse of GenericList succeeds ")]
        public void ExplicitGenericList(string input, bool expectLeadingSeparator, bool expectFirstItem, int expectedRemainingItemCount, bool expectTrailingEmptyItem)
        {
            var parser =
                from leadingSeparator in Parsers.GenericListLeadingSeparator.Optional()
                from firstItem in Parsers.GenericListItem.Once<ExpressionNode>()
                from remainingItems in Parsers.GenericListSeparatorWithItem.Many()
                select new
                {
                    LeadingSeparator = AsFlattenedSequenceIfDefined(leadingSeparator).ToArray(),
                    FirstItem = firstItem.FirstOrDefault(),
                    RemainingItemGroups = remainingItems.Select(items => items.ToArray()).ToArray(),
                    RemainingItems = remainingItems.SelectMany(items => items).ToArray(),
                    AllItemNodes =
                        AsFlattenedSequenceIfDefined(leadingSeparator)
                            .Concat(firstItem)
                            .Concat(
                                remainingItems.SelectMany(items => items)
                            )
                            .ToArray()
                };
            AssertParser.SucceedsWith(parser, input, actual =>
            {
                TestOutput.WriteLine("Input: '{0}'", input);
                TestOutput.WriteLine(
                    new String('=', input.Length + 9)
                );

                foreach (ExpressionNode actualRemainingItem in actual.AllItemNodes)
                {
                    TestOutput.WriteLine("{0} ({1}..{2})",
                        actualRemainingItem.Kind,
                        actualRemainingItem.AbsoluteStart,
                        actualRemainingItem.AbsoluteEnd
                    );
                    if (actualRemainingItem is GenericListItem actualItem)
                        TestOutput.WriteLine("\tValue = '{0}'", actualItem.Value);
                    else if (actualRemainingItem is GenericListSeparator actualSeparator)
                        TestOutput.WriteLine("\tSeparatorOffset = {0}", actualSeparator.SeparatorOffset);
                }

                if (expectLeadingSeparator)
                    Assert.Equal(2, actual.LeadingSeparator.Length); // Leading separator, preceded by pseudo-empty-item

                if (expectFirstItem)
                    Assert.NotNull(actual.FirstItem);

                Assert.Equal(expectedRemainingItemCount * 2, actual.RemainingItems.Length);
                Assert.Equal(expectedRemainingItemCount, actual.RemainingItemGroups.Length);

                if (expectTrailingEmptyItem)
                {
                    GenericListItem lastItem = actual.RemainingItems.OfType<GenericListItem>().Last();
                    Assert.Equal("", lastItem.Value);
                }
            });
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
        Action<ExpressionNode>[] GenericItemTests(string[] expectedValues, Action<string, ExpressionNode> testActionTemplate)
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

        void DumpList(GenericList list, string input)
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
                if (child is GenericListItem actualItem)
                    TestOutput.WriteLine("\tValue = '{0}'", actualItem.Value);
                else if (child is GenericListSeparator actualSeparator)
                    TestOutput.WriteLine("\tSeparatorOffset = {0}", actualSeparator.SeparatorOffset);
            }
        }

        static IEnumerable<T> AsSequenceIfDefined<T>(IOption<T> item)
        {
            if (item.IsDefined)
                yield return item.Get();
        }

        static IEnumerable<T> AsFlattenedSequenceIfDefined<T>(IOption<IEnumerable<T>> items)
        {
            if (items.IsDefined)
            {
                foreach (T item in items.Get())
                    yield return item;
            }
        }
    }
}
