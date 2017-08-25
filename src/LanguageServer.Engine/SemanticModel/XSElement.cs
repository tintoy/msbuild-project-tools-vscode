using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents an XML element.
    /// </summary>
    public abstract class XSElement
        : XSNode<XmlElementSyntaxBase>
    {
        /// <summary>
        ///     The element's attributes (if any).
        /// </summary>
        /// <remarks>
        ///     TODO: Revisit this; make the field read-only once the XSParser can come back to the element and replace it using .WithAttribute().
        /// </remarks>
        ImmutableList<XSAttribute> _attributes = ImmutableList<XSAttribute>.Empty;

        /// <summary>
        ///     Create a new <see cref="XSElement"/>.
        /// </summary>
        /// <param name="element">
        ///     The <see cref="XmlElementSyntaxBase"/> represented by the <see cref="XSElement"/>.
        /// </param>
        /// <param name="range">
        ///     The range, within the source text, spanned by the node.
        /// </param>
        protected XSElement(XmlElementSyntaxBase element, Range range)
            : base(element, range)
        {
        }

        /// <summary>
        ///     The element name.
        /// </summary>
        public string Name => SyntaxNode.Name;

        /// <summary>
        ///     The <see cref="XmlElementSyntaxBase"/> represented by the <see cref="XSElement"/>.
        /// </summary>
        public XmlElementSyntaxBase ElementNode => SyntaxNode;

        /// <summary>
        ///     The kind of XML node represented by the <see cref="XSNode"/>.
        /// </summary>
        public override XSNodeKind Kind => XSNodeKind.Element;

        /// <summary>
        ///     The element's attributes (if any).
        /// </summary>
        public IReadOnlyList<XSAttribute> Attributes => _attributes;

        /// <summary>
        ///     Does the <see cref="XSNode"/> represent valid XML?
        /// </summary>
        public override bool IsValid => true;

        /// <summary>
        ///     Does the <see cref="XSElement"/> have any content (besides attributes).
        /// </summary>
        public abstract bool HasContent { get; }

        /// <summary>
        ///     Does the element have any attributes?
        /// </summary>
        public bool HasAttributes => SyntaxNode.Attributes.Any();

        /// <summary>
        ///     Get the first attribute (if any) with the specified name.
        /// </summary>
        /// <param name="attributeName">
        ///     The attribute name.
        /// </param>
        /// <returns>
        ///     The <see cref="XSAttribute"/>, or <c>null</c> if no attribute was found with the specified name.
        /// </returns>
        public XSAttribute this[string attributeName]
        {
            get => _attributes.FirstOrDefault(attribute => attribute.Name == attributeName);
        }

        /// <summary>
        ///     Add an attribute to the element.
        /// </summary>
        /// <param name="attribute">
        ///     The attribute to add.
        /// </param>
        /// <remarks>
        ///     TODO: Revisit this; make <see cref="XSElement"/> truly immutable and replace this method with .WithAttribute().
        /// </remarks>
        internal void AddAttribute(XSAttribute attribute)
        {
            if (attribute == null)
                throw new System.ArgumentNullException(nameof(attribute));

            _attributes = _attributes.Add(attribute);
        }
    }
}
