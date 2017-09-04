using System;
using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests.ExpressionTests
{
    using SemanticModel.MSBuildExpressions;
    using System.Linq;

    /// <summary>
    ///     Tests for parsing of MSBuild list expressions.
    /// </summary>
    public class ListParserTests
    {
        /// <summary>
        ///     Verify that the <see cref="Parsers.GenericList"/> MSBuild expression parser can parse a simple generic list in an equivalent fashion to <see cref="String.Split(char[])"/>.
        /// </summary>
        [InlineData("ABC")]
        [InlineData(";ABC")]
        [InlineData(";ABC;")]
        [InlineData("ABC;DEF")]
        [InlineData("ABC;DEF;")]
        [Theory(DisplayName = "Parse MSBuild generic list is equivalent to String.Split ")]
        public void GenericListEquivalentToStringSplit(string input)
        {
            string[] expectedValues = input.Split(';');
            Action<ExpressionNode>[] itemTests = GenericItemTests(expectedValues, (expectedValue, actualItem) =>
            {
                Assert.Equal(ExpressionNodeKind.ListItem, actualItem.Kind);
                Assert.Equal(expectedValue, actualItem.Value);
            });

            AssertParser.SucceedsWith(Parsers.GenericList, input, result =>
            {
                Assert.Equal(ExpressionNodeKind.List, result.Kind);
                Assert.Collection(result.Children, itemTests);
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
    }
}
