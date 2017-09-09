using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild grouping expression (parentheses around an expression).
    /// </summary>
    public class GroupExpression
        : ExpressionContainerNode, IPositionAware<GroupExpression>
    {
        /// <summary>
        ///     Create a new <see cref="GroupExpression"/>.
        /// </summary>
        public GroupExpression()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.Group;

        /// <summary>
        ///     The grouped expression.
        /// </summary>
        public ExpressionNode GroupedExpression => Children.Count > 0 ? Children[0] : null;

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
        GroupExpression IPositionAware<GroupExpression>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
