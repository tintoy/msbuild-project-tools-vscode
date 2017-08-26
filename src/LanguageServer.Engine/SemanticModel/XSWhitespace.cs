using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents non-significant whitespace (the syntax model refers to this as whitespace trivia).
    /// </summary>
    public class XSWhitespace
        : XSNode
    {
        /// <summary>
        ///     Create new <see cref="XSWhitespace"/>.
        /// </summary>
        /// <param name="range">
        ///     The <see cref="Range"/>, within the source text, spanned by the whitespace.
        /// </param>
        /// <param name="parent">
        ///     The <see cref="XSNode"/> that contains the whitespace.
        /// </param>
        public XSWhitespace(Range range, XSNode parent)
            : base(range, parent)
        {
        }

        /// <summary>
        ///     The kind of <see cref="XSNode"/>.
        /// </summary>
        public override XSNodeKind Kind => XSNodeKind.Whitespace;

        /// <summary>
        ///     Does the <see cref="XSNode"/> represent valid XML?
        /// </summary>
        public override bool IsValid => true;

        /// <summary>
        ///     Clone the <see cref="XSWhitespace"/>.
        /// </summary>
        /// <returns>
        ///     The clone.
        /// </returns>
        protected override XSNode Clone() => new XSWhitespace(Range, Parent);
    }
}
