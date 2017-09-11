using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     A node in an MSBuild expression tree.
    /// </summary>
    public abstract class ExpressionNode
        : IPositionAware<ExpressionNode>
    {
        /// <summary>
        ///     Create a new <see cref="ExpressionNode"/>.
        /// </summary>
        protected ExpressionNode()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public abstract ExpressionKind Kind { get; }

        /// <summary>
        ///     The node's parent (if any).
        /// </summary>
        public ExpressionNode Parent { get; internal set; }

        /// <summary>
        ///     The node's previous sibling (if any).
        /// </summary>
        public ExpressionNode PreviousSibling { get; internal set; }

        /// <summary>
        ///     The node's next sibling (if any).
        /// </summary>
        public ExpressionNode NextSibling { get; internal set; }

        /// <summary>
        ///     The node's absolute starting position (0-based).
        /// </summary>
        public int AbsoluteStart { get; internal set; }

        /// <summary>
        ///     The node's absolute starting position (0-based).
        /// </summary>
        public int AbsoluteEnd { get; internal set; }

        /// <summary>
        ///     The node's textual range.
        /// </summary>
        public Range Range { get; internal set; } = Range.Empty;

        /// <summary>
        ///     The node's starting position.
        /// </summary>
        public Position Start => Range.Start;

        /// <summary>
        ///     The node's ending position.
        /// </summary>
        public Position End => Range.End;

        /// <summary>
        ///     Get a string representation of the expression node.
        /// </summary>
        /// <returns>
        ///     The string representation.
        /// </returns>
        public override string ToString() => $"MSBuild {Kind} @ {Range}";

        /// <summary>
        ///     Update positioning information.
        /// </summary>
        /// <param name="startPosition">
        ///     The node's starting position.
        /// </param>
        /// <param name="length">
        ///     The node length.
        /// </param>
        protected void SetPosition(Sprache.Position startPosition, int length)
        {
            AbsoluteStart = startPosition.Pos;
            AbsoluteEnd = AbsoluteStart + length;
        }

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
        ExpressionNode IPositionAware<ExpressionNode>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
