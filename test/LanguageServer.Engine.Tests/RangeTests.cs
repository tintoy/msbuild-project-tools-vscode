using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    /// <summary>
    ///     Tests for <see cref="Range"/>.
    /// </summary>
    public class RangeTests
    {
        /// <summary>
        ///     Built-in comparison for <see cref="Range"/>s should result in them being sorted by start position and then end position.
        /// </summary>
        /// <param name="ranges">
        ///     The ranges under test.
        /// </param>
        [MemberData(nameof(TestRanges))]
        [Theory(DisplayName = "Expect ranges to sort by start position, then end position ")]
        public void SortByStartThenEnd(Range[] ranges)
        {
            Range[] expected = ranges
                .OrderBy(range => range.Start)
                .ThenBy(range => range.End)
                .ToArray();

            Array.Sort(ranges);

            Assert.Equal(expected, ranges);
        }

        /// <summary>
        ///     Test data for tests that use ranges.
        /// </summary>
        public static IEnumerable<object[]> TestRanges
        {
            get
            {
                object[] DataRow(params Range[] ranges) => new object[1] { ranges };

                // Simulates node ranges after computing and appending nodes for whitespace.
                yield return DataRow(
                    new Range(1, 1, 7, 12),
                    new Range(2, 5, 5, 16),
                    new Range(2, 15, 2, 34),
                    new Range(3, 9, 3, 21),
                    new Range(4, 9, 4, 21),
                    new Range(6, 5, 6, 26),
                    new Range(1, 11, 2, 5),
                    new Range(5, 16, 6, 5),
                    new Range(6, 26, 7, 1),
                    new Range(2, 35, 3, 9),
                    new Range(3, 21, 4, 9),
                    new Range(4, 21, 5, 5)
                );
            }
        }
    }
}
