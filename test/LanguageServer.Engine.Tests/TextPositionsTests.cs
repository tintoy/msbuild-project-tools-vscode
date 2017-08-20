using System;
using System.Collections.Generic;
using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using Utilities;

    using TheoryData = IEnumerable<object[]>;

    /// <summary>
    ///     Tests for <see cref="TextPositions"/>.
    /// </summary>
    public class TextPositionsTests
    {
        /// <summary>
        ///     The correct 0-based absolute position should be returned, given 1-based line and column numbers.
        /// </summary>
        /// <param name="line">
        ///     The (1-based) target line number.
        /// </param>
        /// <param name="column">
        ///     The (1-based) target column number.
        /// </param>
        /// <param name="expectedChar">
        ///     The character whose (0-based) absolute position in the source text will be used as the basis for the test.
        /// </param>
        [Theory]
        [MemberData(nameof(GetAbsolutePosition_WindowsLineEndings_Data))]
        public void GetAbsolutePosition_WindowsLineEndings(int line, int column, char expectedChar)
        {
            const string text = TestData.TextWithWindowsLineEndings.Text;

            TextPositions textPositions = new TextPositions(text);
            int absolutePosition = textPositions.GetAbsolutePosition(line, column);
            Assert.Equal(expectedChar, text[absolutePosition]);

            Position roundTripped = textPositions.GetPosition(absolutePosition).ToZeroBased();
            Assert.Equal(roundTripped.LineNumber, line);
            Assert.Equal(roundTripped.ColumnNumber, column);
        }
        public static TheoryData GetAbsolutePosition_WindowsLineEndings_Data => TestData.TextWithWindowsLineEndings.GetAbsolutePosition;

        /// <summary>
        ///     The correct line and column
        /// </summary>
        /// <param name="forChar">
        ///     The character whose (0-based) absolute position in the source text will be used as the basis for the test.
        /// </param>
        /// <param name="expectedLine">
        ///     The (1-based) expected line number.
        /// </param>
        /// <param name="expectedColumn">
        ///     The (1-based) expected column number.
        /// </param>
        [Theory]
        [MemberData(nameof(GetPosition_WindowsLineEndings_Data))]
        public void GetPosition_WindowsLineEndings(char forChar, int expectedLine, int expectedColumn)
        {
            const string text = TestData.TextWithWindowsLineEndings.Text;

            int absolutePosition = text.IndexOf(forChar);
            Assert.InRange(absolutePosition, 0, text.Length - 1);

            TextPositions textPositions = new TextPositions(text);
            Position position = textPositions.GetPosition(absolutePosition);
            Assert.True(position.IsOneBased);
            Assert.Equal(expectedLine, position.LineNumber);
            Assert.Equal(expectedColumn, position.ColumnNumber);

            int absolutePositionRoundTripped = textPositions.GetAbsolutePosition(position);
            Assert.Equal(absolutePosition, absolutePositionRoundTripped);
        }
        public static TheoryData GetPosition_WindowsLineEndings_Data => TestData.TextWithWindowsLineEndings.GetPosition;

        /// <summary>
        ///     Text and theory data for use in tests.
        /// </summary>
        public static class TestData
        {
            /// <summary>
            ///     4 lines of text with Windows-style line endings (CR/LF).
            /// </summary>
            public static class TextWithWindowsLineEndings
            {
                /// <summary>
                ///     The text under test.
                /// </summary>
                public const string Text = "123456\r\nABCDEF\r\nGHIJKL\r\nMNOPQR";

                /// <summary>
                ///     The theory data for tests that derive (0-based) absolute position from (1-based) line and column.
                /// </summary>
                public static IEnumerable<object[]> GetAbsolutePosition
                {
                    get
                    {
                        object[] TestData(int line, int column, char expectedChar) => new object[] { line, column, expectedChar };

                        // 123456
                        yield return TestData(line: 0, column: 0, expectedChar: '1');
                        yield return TestData(line: 0, column: 1, expectedChar: '2');
                        yield return TestData(line: 0, column: 2, expectedChar: '3');
                        yield return TestData(line: 0, column: 3, expectedChar: '4');
                        yield return TestData(line: 0, column: 4, expectedChar: '5');
                        yield return TestData(line: 0, column: 5, expectedChar: '6');

                        // ABCDEF
                        yield return TestData(line: 1, column: 0, expectedChar: 'A');
                        yield return TestData(line: 1, column: 1, expectedChar: 'B');
                        yield return TestData(line: 1, column: 2, expectedChar: 'C');
                        yield return TestData(line: 1, column: 3, expectedChar: 'D');
                        yield return TestData(line: 1, column: 4, expectedChar: 'E');
                        yield return TestData(line: 1, column: 5, expectedChar: 'F');

                        // GHIJKL
                        yield return TestData(line: 2, column: 0, expectedChar: 'G');
                        yield return TestData(line: 2, column: 1, expectedChar: 'H');
                        yield return TestData(line: 2, column: 2, expectedChar: 'I');
                        yield return TestData(line: 2, column: 3, expectedChar: 'J');
                        yield return TestData(line: 2, column: 4, expectedChar: 'K');
                        yield return TestData(line: 2, column: 5, expectedChar: 'L');
                    }
                }

                /// <summary>
                ///     The theory data for tests that derive (1-based) line and column from (0-based) absolute position.
                /// </summary>
                public static IEnumerable<object[]> GetPosition
                {
                    get
                    {
                        object[] TestData(char forChar, int expectedLine, int expectedColumn) => new object[] { forChar, expectedLine, expectedColumn };

                        // 123456
                        yield return TestData(forChar: '1', expectedLine: 1, expectedColumn: 1);
                        yield return TestData(forChar: '2', expectedLine: 1, expectedColumn: 2);
                        yield return TestData(forChar: '3', expectedLine: 1, expectedColumn: 3);
                        yield return TestData(forChar: '4', expectedLine: 1, expectedColumn: 4);
                        yield return TestData(forChar: '5', expectedLine: 1, expectedColumn: 5);
                        yield return TestData(forChar: '6', expectedLine: 1, expectedColumn: 6);

                        // ABCDEF
                        yield return TestData(forChar: 'A', expectedLine: 2, expectedColumn: 1);
                        yield return TestData(forChar: 'B', expectedLine: 2, expectedColumn: 2);
                        yield return TestData(forChar: 'C', expectedLine: 2, expectedColumn: 3);
                        yield return TestData(forChar: 'D', expectedLine: 2, expectedColumn: 4);
                        yield return TestData(forChar: 'E', expectedLine: 2, expectedColumn: 5);
                        yield return TestData(forChar: 'F', expectedLine: 2, expectedColumn: 6);
                    }
                }
            }
        }
    }
}
