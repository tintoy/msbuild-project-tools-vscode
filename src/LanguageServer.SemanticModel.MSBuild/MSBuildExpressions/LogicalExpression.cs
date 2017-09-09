using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild logical expression (e.g. And, Or, Not).
    /// </summary>
    public class LogicalExpression
        : ExpressionContainerNode, IPositionAware<LogicalExpression>
    {
        /// <summary>
        ///     Create a new <see cref="LogicalExpression"/>.
        /// </summary>
        public LogicalExpression()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.Logical;

        /// <summary>
        ///     The kind of binary expression represented by the expression.
        /// </summary>
        public LogicalOperatorKind OperatorKind { get; internal set; }

        /// <summary>
        ///     Is the expression a unary expression?
        /// </summary>
        public bool IsUnary => Children.Count == 1;

        /// <summary>
        ///     Is the expression a binary expression?
        /// </summary>
        public bool IsBinary => Children.Count == 2;

        /// <summary>
        ///     The left-hand operand.
        /// </summary>
        public ExpressionNode Left => IsBinary ? Children[0] : null;

        /// <summary>
        ///     The right-hand operand.
        /// </summary>
        public ExpressionNode Right
        {
            get
            {
                if (IsUnary)
                    return Children[0];

                if (IsBinary)
                    return Children[1];

                return null;
            }
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
        LogicalExpression IPositionAware<LogicalExpression>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
