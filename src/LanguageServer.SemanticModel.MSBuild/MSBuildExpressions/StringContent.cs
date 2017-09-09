using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents a run of contiguous characters in an MSBuild quoted-string literal expression.
    /// </summary>
    /// <remarks>
    ///     'Foo $(XXX)' will be parsed as StringContent("Foo ") and Evaluation(Symbol("XXX")).
    /// </remarks>
    public class StringContent
        : ExpressionNode, IPositionAware<StringContent>
    {
        /// <summary>
        ///     Create a new <see cref="StringContent"/>.
        /// </summary>
        public StringContent()
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
        StringContent IPositionAware<StringContent>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
