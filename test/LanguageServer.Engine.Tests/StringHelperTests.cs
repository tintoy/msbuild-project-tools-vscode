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
        [InlineData("ABC;DEF;GHI", 00, ';', "ABC")]
        [InlineData("ABC;DEF;GHI", 01, ';', "ABC")]
        [InlineData("ABC;DEF;GHI", 02, ';', "ABC")]
        [InlineData("ABC;DEF;GHI", 03, ';', "DEF")]
        [InlineData("ABC;DEF;GHI", 04, ';', "DEF")]
        [InlineData("ABC;DEF;GHI", 05, ';', "DEF")]
        [InlineData("ABC;DEF;GHI", 06, ';', "DEF")]
        [InlineData("ABC;DEF;GHI", 07, ';', "GHI")]
        [InlineData("ABC;DEF;GHI", 08, ';', "GHI")]
        [InlineData("ABC;DEF;GHI", 09, ';', "GHI")]
        [InlineData("ABC;DEF;GHI", 10, ';', "GHI")]
        [InlineData(";ABC;DEF;GHI", 00, ';', "ABC")]
        [InlineData(";ABC;DEF;GHI", 01, ';', "ABC")]
        [InlineData(";ABC;DEF;GHI", 02, ';', "ABC")]
        [InlineData(";ABC;DEF;GHI", 03, ';', "ABC")]
        [InlineData(";ABC;DEF;GHI", 04, ';', "DEF")]
        [InlineData(";ABC;DEF;GHI", 05, ';', "DEF")]
        [InlineData(";ABC;DEF;GHI", 06, ';', "DEF")]
        [InlineData(";ABC;DEF;GHI", 07, ';', "DEF")]
        [InlineData(";ABC;DEF;GHI", 08, ';', "GHI")]
        [InlineData(";ABC;DEF;GHI", 09, ';', "GHI")]
        [InlineData(";ABC;DEF;GHI", 10, ';', "GHI")]
        [InlineData(";ABC;DEF;GHI", 11, ';', "GHI")]
        [InlineData("ABC;;DEF", 00, ';', "ABC")]
        [InlineData("ABC;;DEF", 01, ';', "ABC")]
        [InlineData("ABC;;DEF", 02, ';', "ABC")]
        [InlineData("ABC;;DEF", 03, ';', "")]
        [InlineData("ABC;;DEF", 04, ';', "")]
        [InlineData("ABC;;DEF", 05, ';', "DEF")]
        [InlineData("ABC;;DEF", 06, ';', "DEF")]
        [InlineData("ABC;;DEF", 07, ';', "DEF")]
        [InlineData("ABC;;DEF", 08, ';', "DEF")]
        [Theory(DisplayName = "String.DelimitedAt produces correct sub-string ")]
        public void DelimitedSubstring(string input, int targetIndex, char delimiter, string expectedSubstring)
        {
            (int startIndex, int length) = input.DelimitedSegment(targetIndex, delimiter);

            string actualSubstring = input.Substring(startIndex, length);
            Assert.Equal(expectedSubstring, actualSubstring);
        }
    }
}
