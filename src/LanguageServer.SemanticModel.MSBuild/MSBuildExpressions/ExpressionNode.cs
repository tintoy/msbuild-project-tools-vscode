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
        ///     Is the expression represented by the node valid?
        /// </summary>
        public virtual bool IsValid => true;

        /// <summary>
        ///     Does the node represent a "virtual" expression (i.e. one that is present purely to aid intellisense)?
        /// </summary>
        public virtual bool IsVirtual => AbsoluteLength == 0; // Default heuristic considers nodes that take up no space to be virtual.

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
        ///     The node's absolute length, in characters.
        /// </summary>
        public int AbsoluteLength => AbsoluteEnd - AbsoluteStart;

        /// <summary>
        ///     The node's textual range.
        /// </summary>
        public Range Range { get; internal set; } = Range.Zero;

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
