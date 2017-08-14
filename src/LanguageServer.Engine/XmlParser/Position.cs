using System;

namespace MSBuildProjectTools.LanguageServer.XmlParser
{
    /// <summary>
    ///     Represents a position in a text document.
    /// </summary>
    public class Position
        : IEquatable<Position>, IComparable<Position>
    {
        /// <summary>
        ///     Create a new <see cref="position"/>.
        /// </summary>
        /// <param name="lineNumber">
        ///     The line number (1-based).
        /// </param>
        /// <param name="columnNumber">
        ///     The column number (1-based).
        /// </param>
        public Position(int lineNumber, int columnNumber)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        /// <summary>
        ///     The line number (1-based).
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        ///     The column number (1-based).
        /// </summary>
        public int ColumnNumber { get; }

        /// <summary>
        ///     Create a copy of the <see cref="Position"/> with the specified line number.
        /// </summary>
        /// <param name="lineNumber">
        ///     The new line number.
        /// </param>
        /// <returns>
        ///     The new <see cref="Position"/>.
        /// </returns>
        public Position WithLineNumber(int lineNumber) => new Position(lineNumber, ColumnNumber);

        /// <summary>
        ///     Create a copy of the <see cref="Position"/> with the specified column number.
        /// </summary>
        /// <param name="columnNumber">
        ///     The new column number.
        /// </param>
        /// <returns>
        ///     The new <see cref="Position"/>.
        /// </returns>
        public Position WithColumnNumber(int columnNumber) => new Position(LineNumber, columnNumber);

        /// <summary>
        ///     Create a copy of the <see cref="Position"/>, moving by the specified number of lines and / or columns.
        /// </summary>
        /// <param name="lineCount">
        ///     The number of lines (if any) to move by.
        /// </param>
        /// <param name="columnCount">
        ///     The number of columns (if any) to move by.
        /// </param>
        /// <returns>
        ///     The new <see cref="Position"/>.
        /// </returns>
        public Position Move(int lineCount = 0, int columnCount = 0) => new Position(LineNumber + lineCount, ColumnNumber + columnCount);

        /// <summary>
        ///     Determine whether the position is equal to another object.
        /// </summary>
        /// <param name="other">
        ///     The other object.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the position and object are equal; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            return Equals(other as Position);
        }

        /// <summary>
        ///     Get a hash code to represent the position.
        /// </summary>
        /// <returns>
        ///     The hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return LineNumber * 100000 + ColumnNumber;
        }

        /// <summary>
        ///     Determine whether the position is equal to another position.
        /// </summary>
        /// <param name="other">
        ///     The other position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the positions are equal; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Position other)
        {
            if (other == null)
                return false;
            
            return other.LineNumber == LineNumber && other.ColumnNumber == ColumnNumber;
        }

        /// <summary>
        ///     Compare the position to another position.
        /// </summary>
        /// <param name="other">
        ///     The other position.
        /// </param>
        /// <returns>
        ///     0 if the positions are equal, greater than 0 if the other position is less than the current position, less than 0 if the other position is greater than the current position.
        /// </returns>
        public int CompareTo(Position other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            
            int lineComparison = LineNumber.CompareTo(other.LineNumber);
            if (lineComparison != 0)
                return lineComparison;

            return ColumnNumber.CompareTo(other.ColumnNumber);
        }

        /// <summary>
        ///     Test 2 positions for equality.
        /// </summary>
        /// <param name="position1">
        ///     The first position.
        /// </param>
        /// <param name="position2">
        ///     The second position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the positions are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator==(Position position1, Position position2)
        {
            bool isPosition1Null = ReferenceEquals(position1, null);
            bool isPosition2Null = ReferenceEquals(position2, null);
            if (isPosition1Null && isPosition2Null)
                return true;

            if (isPosition1Null || isPosition2Null)
                return false;

            return position1.Equals(position2);
        }

        /// <summary>
        ///     Test 2 positions for inequality.
        /// </summary>
        /// <param name="position1">
        ///     The first position.
        /// </param>
        /// <param name="position2">
        ///     The second position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the positions are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator!=(Position position1, Position position2)
        {
            bool isPosition1Null = ReferenceEquals(position1, null);
            bool isPosition2Null = ReferenceEquals(position2, null);
            if (isPosition1Null && isPosition2Null)
                return false;

            if (isPosition1Null || isPosition2Null)
                return true;

            return !position1.Equals(position2);
        }

        /// <summary>
        ///     Determine if a position is greater than another position.
        /// </summary>
        /// <param name="position1">
        ///     The first position.
        /// </param>
        /// <param name="position2">
        ///     The second position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="position1"/> is greater than <paramref name="position2"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator>(Position position1, Position position2)
        {
            if (position1 == null)
                throw new ArgumentNullException(nameof(position1));

            if (position2 == null)
                throw new ArgumentNullException(nameof(position2));
            
            return position1.CompareTo(position2) > 0;
        }

        /// <summary>
        ///     Determine if a position is greater than another position.
        /// </summary>
        /// <param name="position1">
        ///     The first position.
        /// </param>
        /// <param name="position2">
        ///     The second position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="position1"/> is greater than <paramref name="position2"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator>=(Position position1, Position position2)
        {
            if (position1 == null)
                throw new ArgumentNullException(nameof(position1));

            if (position2 == null)
                throw new ArgumentNullException(nameof(position2));
            
            return position1.CompareTo(position2) >= 0;
        }

        /// <summary>
        ///     Determine if a position is less than another position.
        /// </summary>
        /// <param name="position1">
        ///     The first position.
        /// </param>
        /// <param name="position2">
        ///     The second position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="position1"/> is greater than <paramref name="position2"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator<(Position position1, Position position2)
        {
            if (position1 == null)
                throw new ArgumentNullException(nameof(position1));

            if (position2 == null)
                throw new ArgumentNullException(nameof(position2));
            
            return position1.CompareTo(position2) < 0;
        }

        /// <summary>
        ///     Determine if a position is less than another position.
        /// </summary>
        /// <param name="position1">
        ///     The first position.
        /// </param>
        /// <param name="position2">
        ///     The second position.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="position1"/> is greater than <paramref name="position2"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator<=(Position position1, Position position2)
        {
            if (position1 == null)
                throw new ArgumentNullException(nameof(position1));

            if (position2 == null)
                throw new ArgumentNullException(nameof(position2));
            
            return position1.CompareTo(position2) <= 0;
        }
    }
}
