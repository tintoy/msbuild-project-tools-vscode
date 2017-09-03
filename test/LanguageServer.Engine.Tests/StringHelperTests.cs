using System;
using System.Collections.Generic;
using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using Utilities;

    /// <summary>
    ///     Tests for <see cref="StringHelper"/>.
    /// </summary>
    public class StringHelperTests
    {
        /// <summary>
        ///     Verify that use of <see cref="StringHelper.DelimitedSegment(string, int, char)"/> produces the correct sub-string.
        /// </summary>
        /// <param name="input">
        ///     The input string.
        /// </param>
        /// <param name="targetIndex">
        ///     The index that the delimited sub-string must contain.
        /// </param>
        /// <param name="delimiter">
        ///     The delimiter character.
        /// </param>
        /// <param name="expectedSubstring">
        ///     The expected sub-string.
        /// </param>
        [InlineData("ABC;DEF;GHI", 0, ';', "ABC")]
        [InlineData("ABC;DEF;GHI", 1, ';', "ABC")]
        [InlineData("ABC;DEF;GHI", 2, ';', "ABC")]
        [InlineData("ABC;DEF;GHI", 3, ';', "DEF")]
        [InlineData("ABC;DEF;GHI", 4, ';', "DEF")]
        [InlineData("ABC;DEF;GHI", 10, ';', "GHI")]
        [Theory(DisplayName = "String.DelimitedAt produces correct sub-string ")]
        public void DelimitedSubstring(string input, int targetIndex, char delimiter, string expectedSubstring)
        {
            (int startIndex, int length) = input.DelimitedSegment(targetIndex, delimiter);

            string actualSubstring = input.Substring(startIndex, length);
            Assert.Equal(expectedSubstring, actualSubstring);
        }
    }
}
