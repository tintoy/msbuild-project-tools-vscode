using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild quoted-string literal expression.
    /// </summary>
    public class QuotedStringLiteral
        : ExpressionNode, IPositionAware<QuotedStringLiteral>
    {
        /// <summary>
        ///     Create a new <see cref="QuotedStringLiteral"/>.
        /// </summary>
        public QuotedStringLiteral()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.QuotedString;

        /// <summary>
        ///     The string content.
        /// </summary>
        public string Content { get; set; }

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
        QuotedStringLiteral IPositionAware<QuotedStringLiteral>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
