using Sprache;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
        public abstract ExpressionNodeKind Kind { get; }

        /// <summary>
        ///     The node's parent (if any).
        /// </summary>
        public ExpressionNode Parent { get; internal set; }

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

    /// <summary>
    ///     Represents a generic MSBuild list expression.
    /// </summary>
    public sealed class GenericList
        : ExpressionContainerNode, IPositionAware<GenericList>
    {
        /// <summary>
        ///     Create a new <see cref="GenericList"/>.
        /// </summary>
        public GenericList()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionNodeKind Kind => ExpressionNodeKind.List;

        /// <summary>
        ///     The list's items.
        /// </summary>
        public IEnumerable<GenericListItem> Items => Children.OfType<GenericListItem>();

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
        GenericList IPositionAware<GenericList>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }

    /// <summary>
    ///     Represents a generic MSBuild list expression with leading and trailing whitespace.
    /// </summary>
    public sealed class GenericListSeparator
        : ExpressionNode, IPositionAware<GenericListSeparator>
    {
        /// <summary>
        ///     Create a new <see cref="GenericListSeparator"/>.
        /// </summary>
        public GenericListSeparator()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionNodeKind Kind => ExpressionNodeKind.ListSeparator;

        /// <summary>
        ///     The offset, in characters, of the actual separator character from the <see cref="ExpressionNode.AbsoluteStart"/> of the <see cref="GenericListSeparator"/>.
        /// </summary>
        public int SeparatorOffset { get; internal set; }

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
        GenericListSeparator IPositionAware<GenericListSeparator>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }

    /// <summary>
    ///     Represents a generic MSBuild list expression.
    /// </summary>
    public sealed class GenericListItem
        : ExpressionNode, IPositionAware<GenericListItem>
    {
        /// <summary>
        ///     Create a new <see cref="GenericListItem"/>.
        /// </summary>
        public GenericListItem()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionNodeKind Kind => ExpressionNodeKind.ListItem;

        /// <summary>
        ///     The item value.
        /// </summary>
        public string Value { get; internal set; }

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
        GenericListItem IPositionAware<GenericListItem>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
