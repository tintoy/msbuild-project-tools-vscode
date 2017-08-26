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
            : base(attribute, range, element)
        {
            if (nameRange == null)
                throw new System.ArgumentNullException(nameof(nameRange));

            if (valueRange == null)
                throw new System.ArgumentNullException(nameof(valueRange));

            NameRange = nameRange;
            ValueRange = valueRange;
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
        public XSElement Element => (XSElement)Parent;

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

        /// <summary>
        ///     Create a copy of the <see cref="XSAttribute"/>, but with the specified parent element.
        /// </summary>
        /// <param name="element">
        ///     The parent element, or <c>null</c> if the new node should have no parent.
        /// </param>
        /// <returns>
        ///     The new node.
        /// </returns>
        public XSAttribute WithElement(XSElement element) => (XSAttribute)base.WithParent(element);

        /// <summary>
        ///     Clone the <see cref="XSAttribute"/>.
        /// </summary>
        /// <returns>
        ///     The clone.
        /// </returns>
        protected override XSNode Clone() => new XSAttribute(AttributeNode, Range, NameRange, ValueRange, Element);
    }
}
