using Sprache;
using System.Collections.Immutable;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     A node in an MSBuild expression tree that can have children.
    /// </summary>
    public abstract class ExpressionContainerNode
        : ExpressionNode, IPositionAware<ExpressionContainerNode>
    {
        /// <summary>
        ///     Create a new <see cref="ExpressionContainerNode"/>.
        /// </summary>
        protected ExpressionContainerNode()
        {
        }

        /// <summary>
        ///     The node's children (if any).
        /// </summary>
        public ImmutableList<ExpressionNode> Children { get; internal set; } = ImmutableList<ExpressionNode>.Empty;

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
        ExpressionContainerNode IPositionAware<ExpressionContainerNode>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
