using System;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Extensions for <see cref="String"/>.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        ///     Get the position and length of a delimited sub-string containing the specified index.
        /// </summary>
        /// <param name="str">
        ///     The string to examine.
        /// </param>
        /// <param name="atIndex">
        ///     The index which the delimited sub-string must contain.
        /// </param>
        /// <param name="delimiter">
        ///     The character used to delimit sub-strings.
        /// </param>
        /// <returns>
        ///     The sub-string's starting index and length.
        /// </returns>
        /// <remarks>
        ///     If starting or ending delimiter is not found, the beginning or end of the string will be used.
        /// </remarks>
        public static (int startIndex, int length) DelimitedSegment(this string str, int atIndex, char delimiter)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (atIndex < 0 || atIndex >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(atIndex), atIndex, "Index must be between 0 and 1 less than the string's length.");
            
            if (str.Length == 0)
                return (0, 0);

            int lastIndex = str.Length - 1;
            int afterLastIndex = lastIndex + 1;

            int previousDelimiterIndex;
            if (atIndex > 0)
            {
                previousDelimiterIndex = str.LastIndexOf(delimiter,
                    startIndex: Math.Max(0, atIndex)
                );
                if (previousDelimiterIndex == -1)
                    previousDelimiterIndex = 0; // Indicate that there is no ending delimiter and so the segment starts at the beginning of the string.
                else
                    previousDelimiterIndex++; // Skip over the delimiter itself.
            }
            else
                previousDelimiterIndex = 0;

            int nextDelimiterIndex;
            if (atIndex < lastIndex)
            {
                nextDelimiterIndex = str.IndexOf(delimiter,
                    startIndex: Math.Min(lastIndex, atIndex + 1)
                );
                if (nextDelimiterIndex == -1)
                    nextDelimiterIndex = afterLastIndex; // Indicate that there is no ending delimiter and so the segment runs to the end of the string.
            }
            else
                nextDelimiterIndex = afterLastIndex; // Indicate that there is no ending delimiter and so the segment runs to the end of the string.

            return (
                startIndex: previousDelimiterIndex,
                length: nextDelimiterIndex - previousDelimiterIndex
            );
        }
    }
}
