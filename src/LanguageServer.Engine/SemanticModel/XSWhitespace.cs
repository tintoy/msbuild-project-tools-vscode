using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents non-significant whitespace (the syntax model refers to this as whitespace trivia).
    /// </summary>
    public class XSWhitespace
        : XSNode<SyntaxTrivia>
    {
        /// <summary>
        ///     Create new <see cref="XSWhitespace"/>.
        /// </summary>
        /// <param name="triviaNode">
        ///     The <see cref="SyntaxTrivia"/> represented by the <see cref="XSWhitespace"/>.
        /// </param>
        /// <param name="range">
        ///     The <see cref="Range"/>, within the source text, spanned by the whitespace.
        /// </param>
        protected XSWhitespace(SyntaxTrivia triviaNode, Range range)
            : base(triviaNode, range)
        {
        }

        /// <summary>
        ///     The <see cref="SyntaxTrivia"/> represented by the <see cref="XSWhitespace"/>.
        /// </summary>
        public SyntaxTrivia Trivia => SyntaxNode;

        /// <summary>
        ///     The kind of <see cref="XSNode"/>.
        /// </summary>
        public override XSNodeKind Kind => XSNodeKind.Whitespace;

        /// <summary>
        ///     Does the <see cref="XSNode"/> represent valid XML?
        /// </summary>
        public override bool IsValid => true;
    }
}
