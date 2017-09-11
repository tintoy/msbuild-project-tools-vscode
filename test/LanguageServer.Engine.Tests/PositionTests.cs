using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    /// <summary>
    ///     Tests for <see cref="Position"/>.
    /// </summary>
    public class PositionTests
    {
        /// <summary>
        ///     Verify that <see cref="Position.RelativeTo(Position)"/> gives the correct result from a one-based position to the one-based origin.
        /// </summary>
        /// <param name="line">
        ///     The initial position's line number.
        /// </param>
        /// <param name="column">
        ///     The initial position's column number.
        /// </param>
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [Theory(DisplayName = "Position.RelativeTo is correct between one-based position and origin ")]
        public void OneBased_RelativeTo_Origin(int line, int column)
        {
            Position initialPosition = new Position(line, column);

            Position expected = initialPosition;
            Position actual = initialPosition.RelativeTo(Position.Origin);

            Assert.Equal(expected, actual);
            Assert.True(actual.IsOneBased, "IsOneBased");
        }

        /// <summary>
        ///     Verify that <see cref="Position.RelativeTo(Position)"/> gives the correct result from a one-based position using the one-based origin as its origin.
        /// </summary>
        /// <param name="line">
        ///     The initial position's line number.
        /// </param>
        /// <param name="column">
        ///     The initial position's column number.
        /// </param>
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [Theory(DisplayName = "Position.WithOrigin is correct for one-based position using origin ")]
        public void OneBased_WithOrigin_Origin(int line, int column)
        {
            Position initialPosition = new Position(line, column);

            Position expected = initialPosition;
            Position actual = initialPosition.WithOrigin(Position.Origin);

            Assert.Equal(initialPosition, actual);
            Assert.True(actual.IsOneBased, "IsOneBased");
        }

        /// <summary>
        ///     Verify that <see cref="Position.RelativeTo(Position)"/> gives the correct result from a one-based position to a one-based position.
        /// </summary>
        /// <param name="line">
        ///     The initial position's line number.
        /// </param>
        /// <param name="column">
        ///     The initial position's column number.
        /// </param>
        /// <param name="relativeToLine">
        ///     The base position's line number.
        /// </param>
        /// <param name="relativeToColumn">
        ///     The base position's column number.
        /// </param>
        /// <param name="expectedLine">
        ///     The resulting position's expected line number.
        /// </param>
        /// <param name="expectedColumn">
        ///     The resulting position's expected column number.
        /// </param>
        [InlineData(1, 1, 1, 1, 1, 1)]
        [InlineData(2, 2, 1, 1, 2, 2)]
        [InlineData(3, 3, 1, 1, 3, 3)]
        [InlineData(2, 2, 2, 2, 1, 1)]
        [InlineData(3, 3, 2, 2, 2, 2)]
        [InlineData(5, 5, 2, 2, 4, 4)]
        [InlineData(5, 17, 2, 5, 4, 13)]
        [Theory(DisplayName = "Position.RelativeTo is correct for one-based positions ")]
        public void OneBased_RelativeTo_OneBased(int line, int column, int relativeToLine, int relativeToColumn, int expectedLine, int expectedColumn)
        {
            Position initial = new Position(line, column);
            Position relativeTo = new Position(relativeToLine, relativeToColumn);

            Position expected = new Position(expectedLine, expectedColumn);
            Position actual = initial.RelativeTo(relativeTo);
            Assert.Equal(expected, actual);
            Assert.True(actual.IsOneBased, "IsOneBased");
        }

        /// <summary>
        ///     Verify that <see cref="Position.WithOrigin(Position)"/> gives the correct result from a one-based position to a one-based position.
        /// </summary>
        /// <param name="line">
        ///     The initial position's line number.
        /// </param>
        /// <param name="column">
        ///     The initial position's column number.
        /// </param>
        /// <param name="originLine">
        ///     The origin position's line number.
        /// </param>
        /// <param name="originColumn">
        ///     The origin position's column number.
        /// </param>
        /// <param name="expectedLine">
        ///     The resulting position's expected line number.
        /// </param>
        /// <param name="expectedColumn">
        ///     The resulting position's expected column number.
        /// </param>
        [InlineData(2, 2, 2, 2, 4, 4)]
        [InlineData(5, 17, 2, 5, 6, 21)]
        [Theory(DisplayName = "Position.WithOrigin is correct for one-based positions ")]
        public void OneBased_WithOrigin_OneBased(int line, int column, int originLine, int originColumn, int expectedLine, int expectedColumn)
        {
            Position initialPosition = new Position(line, column);
            Position origin = new Position(originLine, originColumn);

            Position expected = new Position(expectedLine, expectedColumn);
            Position actual = initialPosition.WithOrigin(origin);
            Assert.Equal(expected, actual);
            Assert.True(actual.IsOneBased, "IsOneBased");
        }
    }
}
