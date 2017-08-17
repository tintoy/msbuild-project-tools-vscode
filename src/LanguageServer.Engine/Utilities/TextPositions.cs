using System;
using MSBuildProjectTools.LanguageServer.XmlParser;

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
            
            string[] lines = text.Split(
                separator: new string[] { Environment.NewLine },
                options: StringSplitOptions.None
            );
            _lineLengths = new int[lines.Length];
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                _lineLengths[lineIndex] = lines[lineIndex].Length + Environment.NewLine.Length;
        }

        /// <summary>
        ///     Convert a <see cref="Position"/> to an absolute position within the text.
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
    }
}
