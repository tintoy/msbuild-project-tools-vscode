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

            if (atIndex < 0 || atIndex > str.Length)
                throw new ArgumentOutOfRangeException(nameof(atIndex), atIndex, "Index must be between 0 and the string's length.");
            
            if (str.Length == 0)
                return (0, 0);

            int lastIndex = str.Length - 1;
            int afterLastIndex = lastIndex + 1;

            // They can ask us about the "end" position in the string, but we'll take that to mean the last legal position.
            // This is here DelimitedSegment is often called from the language engine because of VSCode calling us at the end of an attribute value (an index which is after the last valid index for the attribute value string).
            if (atIndex == afterLastIndex)
                atIndex--;

            // Special case: on the first ";" in "A;;B".
            if (atIndex > 0 && str[atIndex - 1] == delimiter && str[atIndex] == delimiter)
                return (atIndex, 0);

            int segmentStartIndex;
            if (atIndex > 0)
            {
                segmentStartIndex = str.LastIndexOf(delimiter,
                    startIndex: atIndex
                );
                if (segmentStartIndex == -1)
                    segmentStartIndex = 0; // Indicate that there is no ending delimiter and so the segment starts at the beginning of the string.
                else
                    segmentStartIndex++; // Skip over the delimiter.
            }
            else if (str[atIndex] == delimiter && lastIndex >= 1)
                segmentStartIndex = 1; // String starts with delimiter; skip over it.
            else
                segmentStartIndex = 0;

            int segmentEndIndex;
            if (atIndex < lastIndex)
            {
                segmentEndIndex = str.IndexOf(delimiter,
                    startIndex: Math.Min(atIndex + 1, lastIndex)
                );
                if (segmentEndIndex == -1)
                    segmentEndIndex = afterLastIndex; // Indicate that there is no ending delimiter and so the segment runs to the end of the string.
            }
            else
                segmentEndIndex = afterLastIndex; // Indicate that there is no ending delimiter and so the segment runs to the end of the string.

            return (
                startIndex: segmentStartIndex,
                length: segmentEndIndex - segmentStartIndex
            );
        }
    }
}
