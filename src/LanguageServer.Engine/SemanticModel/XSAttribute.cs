using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents an XML attribute.
    /// </summary>
    public class XSAttribute
        : XSNode<XmlAttributeSyntax>
    {
        /// <summary>
        ///     Create a new <see cref="XSAttribute"/>.
        /// </summary>
        /// <param name="attribute">
        ///     The <see cref="XmlAttributeSyntax"/> represented by the <see cref="XSAttribute"/>.
        /// </param>
        /// <param name="range">
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute.
        /// </param>
        /// <param name="nameRange">
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute's name.
        /// </param>
        /// <param name="valueRange">
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute's value.
        /// </param>
        /// <param name="element">
        ///     The element that contains the attribute.
        /// </param>
        public XSAttribute(XmlAttributeSyntax attribute, Range range, Range nameRange, Range valueRange, XSElement element)
            : base(attribute, range)
        {
            if (nameRange == null)
                throw new System.ArgumentNullException(nameof(nameRange));

            if (valueRange == null)
                throw new System.ArgumentNullException(nameof(valueRange));

            if (element == null)
                throw new System.ArgumentNullException(nameof(element));

            NameRange = nameRange;
            ValueRange = valueRange;
            Element = element;
        }

        /// <summary>
        ///     The attribute name.
        /// </summary>
        public string Name => AttributeNode.Name;

        /// <summary>
        ///     The attribute value.
        /// </summary>
        public string Value => AttributeNode.Value;

        /// <summary>
        ///     The <see cref="XmlAttributeSyntax"/> represented by the <see cref="XSAttribute"/>.
        /// </summary>
        public XmlAttributeSyntax AttributeNode => SyntaxNode;

        /// <summary>
        ///     The element that contains the attribute.
        /// </summary>
        public XSElement Element { get; }

        /// <summary>
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute's name.
        /// </summary>
        public Range NameRange { get; }

        /// <summary>
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute's value.
        /// </summary>
        public Range ValueRange { get; }

        /// <summary>
        ///     The kind of XML node represented by the <see cref="XSNode"/>.
        /// </summary>
        public override XSNodeKind Kind => XSNodeKind.Attribute;

        /// <summary>
        ///     Does the <see cref="XSNode"/> represent valid XML?
        /// </summary>
        public override bool IsValid => true;
    }
}
