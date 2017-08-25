using Microsoft.Language.Xml;
using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using Utilities;

    // AF: Does not currently work correctly. Redo.

    /// <summary>
    ///     A facility for looking up XML by textual location.
    /// </summary>
    public class XmlLocator
    {
        /// <summary>
        ///     The underlying XML document.
        /// </summary>
        readonly XmlDocumentSyntax _document;

        /// <summary>
        ///     The position-lookup for the underlying XML document text.
        /// </summary>
        readonly TextPositions _documentPositions;

        /// <summary>
        ///     Create a new <see cref="XmlLocator"/>.
        /// </summary>
        /// <param name="document">
        ///     The underlying XML document.
        /// </param>
        /// <param name="documentPositions">
        ///     The position-lookup for the underlying XML document text.
        /// </param>
        public XmlLocator(XmlDocumentSyntax document, TextPositions documentPositions)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            
            if (documentPositions == null)
                throw new ArgumentNullException(nameof(documentPositions));
            
            _document = document;
            _documentPositions = documentPositions;
        }

        /// <summary>
        ///     Inspect the specified position in the XML.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     An <see cref="XmlPosition"/> representing the result of the inspection.
        /// </returns>
        public XmlPosition Inspect(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            
            return Inspect(
                _documentPositions.GetAbsolutePosition(position)
            );
        }

        /// <summary>
        ///     Inspect the specified position in the XML.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position (0-based).
        /// </param>
        /// <returns>
        ///     An <see cref="XmlPosition"/> representing the result of the inspection.
        /// </returns>
        public XmlPosition Inspect(int absolutePosition)
        {
            if (absolutePosition < 0)
                throw new ArgumentOutOfRangeException(nameof(absolutePosition), absolutePosition, "Absolute position cannot be less than 0.");

            SyntaxNode nodeAtPosition = _document.FindNode(absolutePosition);
            if (nodeAtPosition == null)
                return null;

            XmlPositionFlags flags = GetPositionFlags(absolutePosition, nodeAtPosition, existingFlags: XmlPositionFlags.None);
            Position position = _documentPositions.GetPosition(absolutePosition);

            SyntaxNode elementOrAttribute = nodeAtPosition.GetContainingElementOrAttribute();

            return new XmlPosition(position, absolutePosition, flags, nodeAtPosition, elementOrAttribute);
        }

        // AF: This logic is hideous - FIXME.

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestNode">
        ///     The target position's nearest node.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetPositionFlags(int absolutePosition, SyntaxNode nearestNode, XmlPositionFlags existingFlags)
        {
            switch (nearestNode)
            {
                case XmlEmptyElementSyntax emptyElement:
                {
                    return GetEmptyElementPositionFlags(absolutePosition, emptyElement, existingFlags);
                }
                case XmlElementStartTagSyntax elementStartTag:
                {
                    return GetElementStartTagPositionFlags(absolutePosition, elementStartTag, existingFlags);
                }
                case XmlElementEndTagSyntax elementEndTag:
                {
                    return GetElementEndTagPositionFlags(absolutePosition, elementEndTag, existingFlags);
                }
                case XmlElementSyntax element:
                {
                    return GetElementPositionFlags(absolutePosition, element, existingFlags);
                }
                case XmlAttributeSyntax attribute:
                {
                    return GetAttributePositionFlags(absolutePosition, attribute, existingFlags);
                }
                case SyntaxList list:
                {
                    SyntaxNode containingElementOrAttribute = nearestNode.GetContainingElementOrAttribute();

                    if (containingElementOrAttribute is XmlElementSyntax element)
                    {
                        if (element.AttributesNode != null && element.AttributesNode.Span.Contains(absolutePosition))
                            return XmlPositionFlags.ElementAttributes | GetListPositionFlags(absolutePosition, list, existingFlags);
                    }

                    if (containingElementOrAttribute is XmlEmptyElementSyntax emptyElement)
                    {
                        if (emptyElement.AttributesNode != null && emptyElement.AttributesNode.Span.Contains(absolutePosition))
                            return XmlPositionFlags.ElementAttributes | GetListPositionFlags(absolutePosition, list, existingFlags);
                    }

                    if (containingElementOrAttribute is XmlElementStartTagSyntax elementStartTag)
                    {
                        if (elementStartTag.Attributes != null && elementStartTag.Attributes.Span.Contains(absolutePosition))
                            return GetListPositionFlags(absolutePosition, list, existingFlags | XmlPositionFlags.ElementAttributes);
                    }

                    return GetListPositionFlags(absolutePosition, list, existingFlags);
                }
                case XmlNameTokenSyntax nameToken:
                {
                    XmlNameSyntax name = (XmlNameSyntax)nameToken.Parent;
                    if (name.Span.Contains(absolutePosition))
                        return GetNamePositionFlags(absolutePosition, name, existingFlags);

                    SyntaxNode containingElementOrAttribute = name.GetContainingElementOrAttribute();
                    switch (containingElementOrAttribute)
                    {
                        case XmlEmptyElementSyntax emptyElement:
                        {
                            return GetEmptyElementPositionFlags(absolutePosition, emptyElement, existingFlags | XmlPositionFlags.ElementAttributes);
                        }
                        case XmlElementStartTagSyntax elementStartTag:
                        {
                            return GetElementStartTagPositionFlags(absolutePosition, elementStartTag, existingFlags | XmlPositionFlags.ElementAttributes);
                        }
                        case XmlAttributeSyntax attribute:
                        {
                            return GetAttributePositionFlags(absolutePosition, attribute, existingFlags | XmlPositionFlags.ElementAttributes);
                        }
                    }

                    break;
                }
                case SyntaxToken token:
                {
                    bool isPositionBeforeNode = absolutePosition < token.Span.Start;
                    bool isPositionWithinNode = token.Span.Contains(absolutePosition);
                    bool isPositionAfterNode = absolutePosition > token.Span.End;
                    switch (token.Kind)
                    {
                        case SyntaxKind.LessThanToken:
                        {
                            if (token.Parent is XmlEmptyElementSyntax emptyElement)
                                return GetEmptyElementPositionFlags(absolutePosition, emptyElement, existingFlags);

                            if (token.Parent is XmlElementStartTagSyntax elementStartTag)
                            {
                                if (elementStartTag.Span.Contains(absolutePosition))
                                    return GetElementStartTagPositionFlags(absolutePosition, elementStartTag, existingFlags);
                                
                                // We're actually in content of the parent element.
                                return GetPositionFlags(absolutePosition, (SyntaxNode)elementStartTag.ParentElement.Parent, existingFlags | XmlPositionFlags.ElementContent);
                            }

                            break;
                        }
                        case SyntaxKind.GreaterThanToken:
                        case SyntaxKind.SlashGreaterThanToken:
                        {
                            if (token.Parent is XmlEmptyElementSyntax emptyElement)
                            {
                                if (isPositionAfterNode)
                                    return GetEmptyElementPositionFlags(absolutePosition, emptyElement, existingFlags | XmlPositionFlags.ElementContent);
                                else
                                    return GetEmptyElementPositionFlags(absolutePosition, emptyElement, existingFlags);
                            }

                            if (token.Parent is XmlElementStartTagSyntax elementStartTag)
                            {
                                if (isPositionAfterNode)
                                    return GetElementStartTagPositionFlags(absolutePosition, elementStartTag, existingFlags | XmlPositionFlags.ElementContent);
                                else
                                    return GetElementStartTagPositionFlags(absolutePosition, elementStartTag, existingFlags);
                            }

                            if (token.Parent is XmlElementEndTagSyntax elementEndTag)
                            {
                                if (isPositionAfterNode)
                                    return GetElementEndTagPositionFlags(absolutePosition, elementEndTag, existingFlags | XmlPositionFlags.ElementContent);
                                else
                                    return GetElementEndTagPositionFlags(absolutePosition, elementEndTag, existingFlags);
                            }

                            break;
                        }
                    }

                    return GetPositionFlags(absolutePosition, token.Parent, existingFlags);
                }
                case XmlNameSyntax name:
                {
                    if (name.Span.Contains(absolutePosition))
                        return GetNamePositionFlags(absolutePosition, name, existingFlags);

                    SyntaxNode containingElementOrAttribute = name.GetContainingElementOrAttribute();
                    switch (containingElementOrAttribute)
                    {
                        case XmlEmptyElementSyntax emptyElement:
                        {
                            return XmlPositionFlags.ElementAttributes | GetEmptyElementPositionFlags(absolutePosition, emptyElement, existingFlags);
                        }
                        case XmlElementStartTagSyntax elementStartTag:
                        {
                            return XmlPositionFlags.ElementAttributes | GetElementStartTagPositionFlags(absolutePosition, elementStartTag, existingFlags);
                        }
                        case XmlAttributeSyntax attribute:
                        {
                            return XmlPositionFlags.AttributeContent | GetAttributePositionFlags(absolutePosition, attribute, existingFlags);
                        }
                    }

                    break;
                }
            }

            return XmlPositionFlags.None;
        }

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestEmptyElement">
        ///     The target position's nearest empty element.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetEmptyElementPositionFlags(int absolutePosition, XmlEmptyElementSyntax nearestEmptyElement, XmlPositionFlags existingFlags)
        {
            if (nearestEmptyElement == null)
                throw new ArgumentNullException(nameof(nearestEmptyElement));
            
            XmlPositionFlags flags = existingFlags | XmlPositionFlags.Element | XmlPositionFlags.Empty;
            if (nearestEmptyElement.NameNode?.Span.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.Name;
            else if (nearestEmptyElement.AttributesNode?.FullSpan.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.Attribute;

            return flags;
        }

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestElement">
        ///     The target position's nearest element.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetElementPositionFlags(int absolutePosition, XmlElementSyntax nearestElement, XmlPositionFlags existingFlags)
        {
            if (nearestElement == null)
                throw new ArgumentNullException(nameof(nearestElement));

            if (nearestElement.StartTag?.Span.Contains(absolutePosition) ?? false)
                return GetElementStartTagPositionFlags(absolutePosition, nearestElement.StartTag, existingFlags);

            if (nearestElement.EndTag?.Span.Contains(absolutePosition) ?? false)
                return GetElementEndTagPositionFlags(absolutePosition, nearestElement.EndTag, existingFlags);

            XmlPositionFlags flags = existingFlags | XmlPositionFlags.Element;
            if (nearestElement.NameNode?.Span.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.Name;
            else if (nearestElement.AttributesNode?.FullSpan.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.Attribute; // TODO: Recursively resolve this instead.
            else if (nearestElement.Content?.FullSpan.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.ElementContent;
            else if (nearestElement.EndTag?.Span.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.ClosingTag;

            return flags;
        }

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestElementStartTag">
        ///     The target position's nearest element start tag.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetElementStartTagPositionFlags(int absolutePosition, XmlElementStartTagSyntax nearestElementStartTag, XmlPositionFlags existingFlags)
        {
            if (nearestElementStartTag == null)
                throw new ArgumentNullException(nameof(nearestElementStartTag));
            
            XmlPositionFlags flags = existingFlags | XmlPositionFlags.Element | XmlPositionFlags.OpeningTag;
            if (nearestElementStartTag.NameNode?.Span.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.Name;

            return flags;
        }

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestElementEndTag">
        ///     The target position's nearest element end tag.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetElementEndTagPositionFlags(int absolutePosition, XmlElementEndTagSyntax nearestElementEndTag, XmlPositionFlags existingFlags)
        {
            if (nearestElementEndTag == null)
                throw new ArgumentNullException(nameof(nearestElementEndTag));

            XmlPositionFlags flags = existingFlags | XmlPositionFlags.Element | XmlPositionFlags.OpeningTag;
            if (nearestElementEndTag.NameNode?.Span.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.Name;

            return flags;
        }

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestList">
        ///     The target position's nearest list.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetListPositionFlags(int absolutePosition, SyntaxList nearestList, XmlPositionFlags existingFlags)
        {
            if (nearestList == null)
                throw new ArgumentNullException(nameof(nearestList));

            SyntaxNode elementOrAttributeAtPosition = nearestList.GetContainingElementOrAttribute();
            if (elementOrAttributeAtPosition == null)
                return existingFlags;

            if (elementOrAttributeAtPosition is XmlElementSyntaxBase)
                return XmlPositionFlags.ElementContent | GetPositionFlags(absolutePosition, elementOrAttributeAtPosition, existingFlags);

            if (elementOrAttributeAtPosition is XmlAttributeSyntax)
                return GetPositionFlags(absolutePosition, elementOrAttributeAtPosition, existingFlags);

            return XmlPositionFlags.None;
        }

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestName">
        ///     The target position's nearest name.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetNamePositionFlags(int absolutePosition, XmlNameSyntax nearestName, XmlPositionFlags existingFlags)
        {
            if (nearestName == null)
                throw new ArgumentNullException(nameof(nearestName));

            switch (nearestName.Parent)
            {
                case XmlEmptyElementSyntax emptyElement:
                {
                    return XmlPositionFlags.Name | GetEmptyElementPositionFlags(absolutePosition, emptyElement, existingFlags);
                }
                case XmlElementSyntax element:
                {
                    return XmlPositionFlags.Name | GetElementPositionFlags(absolutePosition, element, existingFlags);
                }
                case XmlElementStartTagSyntax elementStartTag:
                {
                    return XmlPositionFlags.Name | GetElementStartTagPositionFlags(absolutePosition, elementStartTag, existingFlags);
                }
                case XmlElementEndTagSyntax elementEndTag:
                {
                    return XmlPositionFlags.Name | GetElementEndTagPositionFlags(absolutePosition, elementEndTag, existingFlags);
                }
                case XmlAttributeSyntax attribute:
                {
                    return XmlPositionFlags.Name | GetAttributePositionFlags(absolutePosition, attribute, existingFlags);
                }
            }

            return XmlPositionFlags.None;
        }

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the specified position.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position.
        /// </param>
        /// <param name="nearestAttribute">
        ///     The target position's nearest attribute.
        /// </param>
        /// <param name="existingFlags">
        ///     Existing flags (if any).
        /// </param>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags GetAttributePositionFlags(int absolutePosition, XmlAttributeSyntax nearestAttribute, XmlPositionFlags existingFlags)
        {
            if (nearestAttribute == null)
                throw new ArgumentNullException(nameof(nearestAttribute));

            XmlPositionFlags flags = existingFlags | XmlPositionFlags.Attribute;
            if (nearestAttribute?.NameNode.Span.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.Name;
            else if (nearestAttribute?.ValueNode.Span.Contains(absolutePosition) ?? false)
                flags |= XmlPositionFlags.ElementContent;

            return flags;
        }
    }
}
