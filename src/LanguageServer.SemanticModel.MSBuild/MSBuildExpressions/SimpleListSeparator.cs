using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents a simple MSBuild list separator with leading and trailing whitespace.
    /// </summary>
    public sealed class SimpleListSeparator
        : ExpressionNode, IPositionAware<SimpleListSeparator>
    {
        /// <summary>
        ///     Create a new <see cref="SimpleListSeparator"/>.
        /// </summary>
        public SimpleListSeparator()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.ListSeparator;

        /// <summary>
        ///     The offset, in characters, of the actual separator character from the <see cref="ExpressionNode.AbsoluteStart"/> of the <see cref="SimpleListSeparator"/>.
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
        SimpleListSeparator IPositionAware<SimpleListSeparator>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
