using Sprache;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents a simple MSBuild list expression.
    /// </summary>
    public sealed class SimpleList
        : ExpressionContainerNode, IPositionAware<SimpleList>
    {
        /// <summary>
        ///     Create a new <see cref="SimpleList"/>.
        /// </summary>
        public SimpleList()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.List;

        /// <summary>
        ///     The list's items.
        /// </summary>
        public IEnumerable<SimpleListItem> Items => Children.OfType<SimpleListItem>();

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
        SimpleList IPositionAware<SimpleList>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
