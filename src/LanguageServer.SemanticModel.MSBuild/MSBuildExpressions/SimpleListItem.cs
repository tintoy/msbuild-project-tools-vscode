using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents a simple MSBuild list item.
    /// </summary>
    public sealed class SimpleListItem
        : ExpressionNode, IPositionAware<SimpleListItem>
    {
        /// <summary>
        ///     Create a new <see cref="SimpleListItem"/>.
        /// </summary>
        public SimpleListItem()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.SimpleListItem;

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
        SimpleListItem IPositionAware<SimpleListItem>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
