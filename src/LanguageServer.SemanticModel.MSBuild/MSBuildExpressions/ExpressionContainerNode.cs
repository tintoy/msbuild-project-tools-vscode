using Sprache;
using System.Collections.Immutable;
using System;

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
        ///     Get the child expression at the specified index.
        /// </summary>
        /// <typeparam name="TChild">
        ///     The type of child expression to retrieve.
        /// </typeparam>
        /// <param name="childIndex">
        ///     The index of the child expression to retrieve.
        /// </param>
        /// <returns>
        ///     The child expression.
        /// </returns>
        protected TChild GetChild<TChild>(int childIndex)
            where TChild : ExpressionNode
        {
            if (childIndex < 0 || childIndex >= Children.Count)
                throw new ArgumentOutOfRangeException(nameof(childIndex), childIndex, $"There is no child expression at index {childIndex}.");

            return (TChild)Children[childIndex];
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
        ExpressionContainerNode IPositionAware<ExpressionContainerNode>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
