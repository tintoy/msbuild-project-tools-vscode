using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents the root of an MSBuild expression tree.
    /// </summary>
    public class ExpressionTree
        : ExpressionContainerNode, IPositionAware<ExpressionTree>
    {
        /// <summary>
        ///     Create a new <see cref="ExpressionTree"/>.
        /// </summary>
        public ExpressionTree()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.Root;

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
        ExpressionTree IPositionAware<ExpressionTree>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
