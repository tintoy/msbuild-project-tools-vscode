using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an empty MSBuild list item.
    /// </summary>
    public sealed class EmptyListItem
        : ExpressionNode, IPositionAware<EmptyListItem>
    {
        /// <summary>
        ///     Create a new <see cref="EmptyListItem"/>.
        /// </summary>
        public EmptyListItem()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.EmptyListItem;

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
        EmptyListItem IPositionAware<EmptyListItem>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
