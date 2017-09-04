using Sprache;
using System.Collections.Immutable;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     A node in an MSBuild expression tree.
    /// </summary>
    public class ExpressionNode
        : IPositionAware<ExpressionNode>
    {
        /// <summary>
        ///     Create a new <see cref="ExpressionNode"/>.
        /// </summary>
        public ExpressionNode()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public ExpressionNodeKind Kind { get; internal set; }

        /// <summary>
        ///     The node value.
        /// </summary>
        public string Value { get; internal set; }

        /// <summary>
        ///     The node's children (if any).
        /// </summary>
        public ImmutableList<ExpressionNode> Children { get; internal set; } = ImmutableList<ExpressionNode>.Empty;

        /// <summary>
        ///     The node's absolute starting position (0-based).
        /// </summary>
        public int AbsoluteStart { get; internal set; }

        /// <summary>
        ///     The node's absolute starting position (0-based).
        /// </summary>
        public int AbsoluteEnd { get; internal set; }

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
            AbsoluteStart = startPosition.Pos;
            AbsoluteEnd = AbsoluteStart + length;
            
            return this;
        }
    }
}
