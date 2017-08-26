using Microsoft.Language.Xml;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents an XML element.
    /// </summary>
    public abstract class XSElement
        : XSNode<XmlElementSyntaxBase>
    {
        /// <summary>
        ///     Create a new <see cref="XSElement"/>.
        /// </summary>
        /// <param name="element">
        ///     The <see cref="XmlElementSyntaxBase"/> represented by the <see cref="XSElement"/>.
        /// </param>
        /// <param name="range">
        ///     The range, within the source text, spanned by the node.
        /// </param>
        /// <param name="parent">
        ///     The <see cref="XSElement"/>'s parent element (if any).
        /// </param>
        protected XSElement(XmlElementSyntaxBase element, Range range, XSElement parent)
            : base(element, range, parent)
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
        ///     The <see cref="XSElement"/>'s parent element (if any).
        /// </summary>
        public XSElement ParentElement => (XSElement)Parent;

        /// <summary>
        ///     The kind of XML node represented by the <see cref="XSNode"/>.
        /// </summary>
        public override XSNodeKind Kind => XSNodeKind.Element;

        /// <summary>
        ///     The element's attributes (if any).
        /// </summary>
        public ImmutableList<XSAttribute> Attributes { get; private set; } = ImmutableList<XSAttribute>.Empty;

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
            get => Attributes.FirstOrDefault(attribute => attribute.Name == attributeName);
        }

        /// <summary>
        ///     Create a copy of the <see cref="XSElement"/>, adding the specified attribute.
        /// </summary>
        /// <param name="attribute">
        ///     The attribute to add.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSElement"/>.
        /// </returns>
        public XSElement WithAttribute(XSAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            
            XSElement clone = (XSElement)Clone();
            clone.Attributes = clone.Attributes.Add(
                attribute.WithElement(clone)
            );

            return clone;
        }

        /// <summary>
        ///     Create a copy of the <see cref="XSElement"/>, but with the specified attributes.
        /// </summary>
        /// <param name="attributes">
        ///     The attributes (if any).
        /// </param>
        /// <returns>
        ///     The new <see cref="XSElement"/>.
        /// </returns>
        public XSElement WithAttributes(IEnumerable<XSAttribute> attributes)
        {
            XSElement clone = (XSElement)Clone();

            if (attributes != null)
            {
                // AF: Super-ugly, FIXME.
                clone.Attributes = ImmutableList.CreateRange(
                    attributes.Select(
                        attribute => attribute.WithElement(clone)
                    )
                );
            }
            else
                clone.Attributes = ImmutableList<XSAttribute>.Empty;

            return clone;
        }

        /// <summary>
        ///     Create a copy of the <see cref="XSElement"/>, but with the specified parent element.
        /// </summary>
        /// <param name="parentElement">
        ///     The parent <see cref="XSElement"/>, or <c>null</c> if the new element will have no parent element.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSElement"/>.
        /// </returns>
        public XSElement WithParentElement(XSElement parentElement) => (XSElement)base.WithParent(parentElement);
    }
}
