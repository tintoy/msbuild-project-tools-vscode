using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents an XML element with content.
    /// </summary>
    public class XSElementWithContent
        : XSElement
    {
        /// <summary>
        ///     The range, within the source text, spanned by the node.
        /// </summary>
        /// <param name="element">
        ///     The <see cref="XmlElementSyntax"/> represented by the <see cref="XSElementWithContent"/>.
        /// </param>
        /// <param name="range">
        ///     The <see cref="Range"/>, within the source text, spanned by the element and its content.
        /// </param>
        /// <param name="openingTagRange">
        ///     The <see cref="Range"/>, within the source text, spanned by the element's opening tag.
        /// </param>
        /// <param name="contentRange">
        ///     The <see cref="Range"/>, within the source text, spanned by the element's content.
        /// </param>
        /// <param name="closingTagRange">
        ///     The <see cref="Range"/>, within the source text, spanned by the element's closing tag.
        /// </param>
        public XSElementWithContent(XmlElementSyntax element, Range range, Range openingTagRange, Range contentRange, Range closingTagRange)
            : base(element, range)
        {
            if (openingTagRange == null)
                throw new System.ArgumentNullException(nameof(openingTagRange));

            if (contentRange == null)
                throw new System.ArgumentNullException(nameof(contentRange));

            if (closingTagRange == null)
                throw new System.ArgumentNullException(nameof(closingTagRange));

            OpeningTagRange = openingTagRange;
            ContentRange = contentRange;
            ClosingTagRange = closingTagRange;
        }

        /// <summary>
        ///     The <see cref="XmlElementSyntax"/> represented by the <see cref="XSElementWithContent"/>.
        /// </summary>
        public new XmlElementSyntax ElementNode => (XmlElementSyntax)SyntaxNode;

        /// <summary>
        ///     The <see cref="Range"/>, within the source text, spanned by the element's opening tag.
        /// </summary>
        public Range OpeningTagRange { get; }

        /// <summary>
        ///     The <see cref="Range"/>, within the source text, spanned by the element's content.
        /// </summary>
        public Range ContentRange { get; }

        /// <summary>
        ///     The <see cref="Range"/>, within the source text, spanned by the element's closing tag.
        /// </summary>
        public Range ClosingTagRange { get; }

        /// <summary>
        ///     The kind of XML node represented by the <see cref="XSNode"/>.
        /// </summary>
        public override XSNodeKind Kind => XSNodeKind.Element;

        /// <summary>
        ///     Does the <see cref="XSNode"/> represent valid XML?
        /// </summary>
        public override bool IsValid => true;

        /// <summary>
        ///     Does the <see cref="XSElement"/> have any content (besides attributes)?
        /// </summary>
        public override bool HasContent => true;
    }
}
