using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild evaluation expression.
    /// </summary>
    public class Evaluation
        : ExpressionContainerNode, IPositionAware<Evaluation>
    {
        /// <summary>
        ///     Create a new <see cref="Evaluation"/>.
        /// </summary>
        public Evaluation()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.QuotedString;

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
        Evaluation IPositionAware<Evaluation>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
