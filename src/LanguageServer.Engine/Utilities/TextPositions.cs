using System;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     A quick-and-dirty calculator for text positions.
    /// </summary>
    /// <remarks>
    ///     This could easily be improved by also storing a character sub-total for each line.
    /// </remarks>
    public sealed class TextPositions
    {
        /// <summary>
        ///     The lengths of each line of the text.
        /// </summary>
        readonly int[] _lineLengths;

        /// <summary>
        ///     Create a new <see cref="TextPositions"/> for the specified text.
        /// </summary>
        /// <param name="text">
        ///     The text.
        /// </param>
        public TextPositions(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            HasWindowsLineEndings = text.Contains("\r\n") || !text.Contains("\n");
            LineEnding = HasWindowsLineEndings ? "\r\n" : "\n";
            
            string[] lines = text.Split(
                separator: new string[] { LineEnding },
                options: StringSplitOptions.None
            );
            _lineLengths = new int[lines.Length];
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                _lineLengths[lineIndex] = lines[lineIndex].Length + LineEnding.Length;
        }

        /// <summary>
        ///     The number of lines in the text.
        /// </summary>
        public int LineCount => _lineLengths.Length;

        /// <summary>
        ///     Does the text have Windows-style line endines (CRLF)?
        /// </summary>
        public bool HasWindowsLineEndings { get; }

        /// <summary>
        ///     The line ending used by the text.
        /// </summary>
        public string LineEnding { get; set; }

        /// <summary>
        ///     Convert a <see cref="Position"/> to an absolute position within the text.
        /// </summary>
        /// <param name="position">
        ///     The target <see cref="Position"/>.
        /// </param>
        /// <returns>
        ///     The equivalent absolute position within the text.
        /// </returns>
        public int GetAbsolutePosition(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            position = position.ToOneBased();

            return GetAbsolutePosition(position.LineNumber, position.ColumnNumber);
        }

        /// <summary>
        ///     Convert line and column numbers to an absolute position within the text.
        /// </summary>
        /// <param name="line">
        ///     The target line (1-based).
        /// </param>
        /// <param name="column">
        ///     The target column (1-based).
        /// </param>
        /// <returns>
        ///     The equivalent absolute position within the text.
        /// </returns>
        public int GetAbsolutePosition(int line, int column)
        {
            if (line < 1)
                throw new ArgumentOutOfRangeException(nameof(line), line, "Line cannot be less than 1.");

            if (column < 1)
                throw new ArgumentOutOfRangeException(nameof(column), column, "Column cannot be less than 1.");

            // Indexes are 0-based.
            int targetLine = line - 1;
            int targetColumn = column - 1;

            if (targetLine >= _lineLengths.Length)
                throw new ArgumentOutOfRangeException(nameof(line), line, "Line is past the end of the text.");

            if (targetColumn >= _lineLengths[targetLine])
                throw new ArgumentOutOfRangeException(nameof(column), column, "Column is past the end of the line.");

            if (targetLine == 0)
                return targetColumn;

            // Position up to preceding line.
            int targetPosition = 0;
            for (int lineIndex = 0; lineIndex < targetLine; lineIndex++)
                targetPosition += _lineLengths[lineIndex];

            // And the final line.
            targetPosition += targetColumn;

            return targetPosition;
        }

        /// <summary>
        ///     Convert an absolute position to a line and column in the text.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The absolute position (0-based).
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Position"/> within the text.
        /// </returns>
        public Position GetPosition(int absolutePosition)
        {
            int targetLine = -1;
            int targetColumn = -1;

            int remaining = absolutePosition;
            for (int lineIndex = 0; lineIndex < _lineLengths.Length; lineIndex++)
            {
                int currentLineLength = _lineLengths[lineIndex];

                if (remaining + 1 < currentLineLength)
                {
                    targetLine = lineIndex;
                    targetColumn = remaining;

                    break;
                }

                remaining -= currentLineLength;
                if (remaining < 0)
                {
                    // BUG - this returns incorrect results for absolute positions at the end of the line.
                    targetLine = lineIndex + 1;
                    targetColumn = -1; // => 0

                    break;
                }
            }

            // Lines and columns are 1-based.
            return Position.FromZeroBased(targetLine, targetColumn).ToOneBased();
        }
    }
}
