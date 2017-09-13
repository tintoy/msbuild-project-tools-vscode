using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild evaluation expression.
    /// </summary>
    public class Evaluate
        : ExpressionContainerNode, IPositionAware<Evaluate>
    {
        /// <summary>
        ///     Create a new <see cref="Evaluate"/>.
        /// </summary>
        public Evaluate()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.Evaluate;

        /// <summary>
        ///     Is the evaluation expression valid (i.e. has exactly one child)?
        /// </summary>
        public override bool IsValid => Children.Count == 1 && base.IsValid;

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
        Evaluate IPositionAware<Evaluate>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
