using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild comparison expression.
    /// </summary>
    public class SymbolExpression
        : ExpressionNode, IPositionAware<SymbolExpression>
    {
        /// <summary>
        ///     Create a new <see cref="SymbolExpression"/>.
        /// </summary>
        public SymbolExpression()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.Symbol;

        /// <summary>
        ///     The symbol name.
        /// </summary>
        public string Name { get; internal set; }

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
        SymbolExpression IPositionAware<SymbolExpression>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
