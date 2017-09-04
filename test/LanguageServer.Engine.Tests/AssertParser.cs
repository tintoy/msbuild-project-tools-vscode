using Sprache;
using System.Collections.Generic;
using Xunit;
using System;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Assertions for testing parsers.
    /// </summary>
    public static class AssertParser
    {
        /// <summary>
        ///     Assert that a parser succeeds with a single result.
        /// </summary>
        /// <param name="parser">
        ///     The parser under test.
        /// </param>
        /// <param name="input">
        ///     The test input.
        /// </param>
        /// <param name="expectedResult">
        ///     The expected result.
        /// </param>
        public static void SucceedsWithOne<TResult>(Parser<IEnumerable<TResult>> parser, string input, TResult expectedResult)
        {
            SucceedsWith(parser, input, actualResults =>
            {
                Assert.Collection(actualResults,
                    singleResult => Assert.Equal(expectedResult, singleResult)
                );
            });
        }

        /// <summary>
        ///     Assert that a parser succeeds with 1 or more results.
        /// </summary>
        /// <param name="parser">
        ///     The parser under test.
        /// </param>
        /// <param name="input">
        ///     The test input.
        /// </param>
        /// <param name="expectedResults">
        ///     The expected results.
        /// </param>
        public static void SucceedsWithMany<TResult>(Parser<IEnumerable<TResult>> parser, string input, IEnumerable<TResult> expectedResults)
        {
            SucceedsWith(parser, input, actualResults =>
            {
                Assert.Equal(expectedResults, actualResults);
            });
        }

        /// <summary>
        ///     Assert that a parser successfully parses all input.
        /// </summary>
        /// <param name="parser">
        ///     The parser under test.
        /// </param>
        /// <param name="input">
        ///     The test input.
        /// </param>
        public static void SucceedsWithAll(Parser<IEnumerable<char>> parser, string input)
        {
            SucceedsWithMany(parser, input, input.ToCharArray());
        }

        /// <summary>
        ///     Assert that a parser succeeds with the specified result.
        /// </summary>
        /// <param name="parser">
        ///     The parser under test.
        /// </param>
        /// <param name="input">
        ///     The test input.
        /// </param>
        /// <param name="resultAssertion">
        ///     An action that makes assertions about the result.
        /// </param>
        public static void SucceedsWith<TResult>(Parser<TResult> parser, string input, Action<TResult> resultAssertion)
        {
            IResult<TResult> result = parser.TryParse(input);
            Assert.True(result.WasSuccessful, $"Parsing of '{input}' failed unexpectedly ({String.Join(", ", result.Expectations)}).");

            resultAssertion(result.Value);
        }

        /// <summary>
        ///     Assert that the parser fails to parse the specified input.
        /// </summary>
        /// <param name="parser">
        ///     The parser under test.
        /// </param>
        /// <param name="input">
        ///     The test input.
        /// </param>
        public static void Fails<T>(Parser<T> parser, string input)
        {
            FailsWith(parser, input, failureResult => { });
        }

        /// <summary>
        ///     Assert that the parser fails to parse the specified input.
        /// </summary>
        /// <param name="parser">
        ///     The parser under test.
        /// </param>
        /// <param name="input">
        ///     The test input.
        /// </param>
        /// <param name="position">
        ///     The position at which parsing is expected to fail.
        /// </param>
        public static void FailsAt<T>(Parser<T> parser, string input, int position)
        {
            FailsWith(parser, input, failureResult =>
            {
                Assert.Equal(position, failureResult.Remainder.Position);
            });
        }

        /// <summary>
        ///     Assert that a parser fails with the specified result.
        /// </summary>
        /// <param name="parser">
        ///     The parser under test.
        /// </param>
        /// <param name="input">
        ///     The test input.
        /// </param>
        /// <param name="resultAssertion">
        ///     An action that makes assertions about the result.
        /// </param>
        public static void FailsWith<T>(Parser<T> parser, string input, Action<IResult<T>> resultAssertion)
        {
            IResult<T> result = parser.TryParse(input);
            Assert.True(result.WasSuccessful, $"Parsing of '{input}' succeeded unexpectedly ('{result.Value}').");

            resultAssertion(result);
        }
    }
}
