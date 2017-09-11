using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild comparison expression.
    /// </summary>
    public class Compare
        : ExpressionContainerNode, IPositionAware<Compare>
    {
        /// <summary>
        ///     Create a new <see cref="Compare"/>.
        /// </summary>
        public Compare()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.Compare;

        /// <summary>
        ///     The kind of comparison represented by the expression.
        /// </summary>
        public ComparisonKind ComparisonKind { get; internal set; }

        /// <summary>
        ///     The left-hand operand.
        /// </summary>
        public ExpressionNode Left => Children[0];

        /// <summary>
        ///     The right-hand operand.
        /// </summary>
        public ExpressionNode Right => Children[1];

        /// <summary>
        ///     Get a string representation of the expression node.
        /// </summary>
        /// <returns>
        ///     The string representation.
        /// </returns>
        public override string ToString() => $"MSBuild Compare ({ComparisonKind}) expression @ {Range}";

        /// <summary>
        ///     Update positioning information.
        /// </summary>
        /// <param name="startPosition">
        ///     The node's starting position.
        /// </param>
        /// <param name="length">
        ///     The node length.
        /// </param>
        /// <returns>
        ///     The <see cref="ExpressionNode"/>.
        /// </returns>
        Compare IPositionAware<Compare>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
