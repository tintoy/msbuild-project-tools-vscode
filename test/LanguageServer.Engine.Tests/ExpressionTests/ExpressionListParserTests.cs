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
    public class ExpressionListParserTests
    {
        /// <summary>
        ///     Create a new expression list parser test-suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public ExpressionListParserTests(ITestOutputHelper testOutput)
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
            AssertParser.SucceedsWith(Parsers.SimpleLists.Item, input, actualItem =>
            {
                Assert.Equal(input, actualItem.Value);
            });
        }

        // TODO: Implement tests!

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
                else if (child is ListSeparator actualSeparator)
                    TestOutput.WriteLine("\tSeparatorOffset = {0}", actualSeparator.SeparatorOffset);
            }
        }
    }
}
