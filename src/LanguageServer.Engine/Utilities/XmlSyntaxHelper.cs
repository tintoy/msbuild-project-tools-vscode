using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Extension methods for working with types from Microsoft.Language.Xml.
    /// </summary>
    public static class XmlSyntaxHelper
    {
        /// <summary>
        ///     Enumerate the element's descendant elements.
        /// </summary>
        /// <param name="element">
        ///     The target element.
        /// </param>
        /// <returns>
        ///     A sequence of descendant elements.
        /// </returns>
        public static IEnumerable<IXmlElement> Descendants(this IXmlElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            foreach (var childElement in element.Elements)
            {
                yield return childElement;

                foreach (var descendantElement in childElement.Descendants())
                    yield return descendantElement;
            }
        }

        /// <summary>
        ///     Enumerate the elements' descendant elements.
        /// </summary>
        /// <param name="elements">
        ///     The target elements.
        /// </param>
        /// <returns>
        ///     A sequence of descendant elements.
        /// </returns>
        public static IEnumerable<IXmlElement> Descendants(this IEnumerable<IXmlElement> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            return elements.SelectMany(
                element => element.Descendants()
            );
        }

        /// <summary>
        ///     Enumerate the element's ancestors.
        /// </summary>
        /// <param name="element">
        ///     The target element.
        /// </param>
        /// <returns>
        ///     A sequence of ancestor elements.
        /// </returns>
        public static IEnumerable<IXmlElement> Ancestors(this IXmlElement element)
        {
            IXmlElement parent = element.Parent;
            if (parent == null)
                yield break;

            do
                yield return parent;
            while ((parent = parent.Parent) != null);
        }

        /// <summary>
        ///     Enumerate the syntax node's descendent nodes.
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <returns>
        ///     A sequence of descendent syntax nodes.
        /// </returns>
        public static IEnumerable<SyntaxNode> DescendantNodes(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            foreach (SyntaxNode childNode in syntaxNode.ChildNodes)
            {
                yield return childNode;

                foreach (SyntaxNode descendantNode in childNode.DescendantNodes())
                    yield return childNode;
            }
        }

        /// <summary>
        ///     Enumerate the syntax node's ancestor nodes.
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <returns>
        ///     A sequence of ancestor syntax nodes.
        /// </returns>
        public static IEnumerable<SyntaxNode> AncestorNodes(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            SyntaxNode parent = syntaxNode.Parent;
            if (parent == null)
                yield break;

            do
                yield return parent;
            while ((parent = parent.Parent) != null);
        }

        /// <summary>
        ///     Get the syntax node's first ancestor of the specified type.
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <returns>
        ///     The ancestor node, or <c>null</c> if no ancestor of the specified type was found.
        /// </returns>
        public static TSyntax GetFirstParentOfType<TSyntax>(this SyntaxNode syntaxNode)
        {
            return syntaxNode.AncestorNodes().OfType<TSyntax>().FirstOrDefault();
        }

        /// <summary>
        ///     Get the syntax node's nearest containing element (if any).
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <returns>
        ///     The containing element, or <c>null</c> the syntax node is not a child of an element.
        /// </returns>
        public static XmlElementSyntaxBase GetContainingElement(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            if (syntaxNode is XmlElementSyntaxBase element)
                return element;

            return syntaxNode.GetFirstParentOfType<XmlElementSyntaxBase>();
        }

        /// <summary>
        ///     Get the syntax node's nearest containing attribute (if any).
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <returns>
        ///     The containing attribute, or <c>null</c> the syntax node is not a child of an attribute.
        /// </returns>
        public static XmlAttributeSyntax GetContainingAttribute(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            if (syntaxNode is XmlAttributeSyntax attribute)
                return attribute;

            return syntaxNode.GetFirstParentOfType<XmlAttributeSyntax>();
        }

        /// <summary>
        ///     Get the syntax node's first ancestor of the specified kind.
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <param name="syntaxKind">
        ///     The kind of node to find.
        /// </param>
        /// <returns>
        ///     The ancestor node, or <c>null</c> if no ancestor of the specified kind was found.
        /// </returns>
        public static SyntaxNode GetFirstParentOfKind(this SyntaxNode syntaxNode, SyntaxKind syntaxKind)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            if (syntaxNode.Kind == syntaxKind)
                return syntaxNode;

            return syntaxNode.AncestorNodes().FirstOrDefault(node => node.Kind == syntaxKind);
        }

        /// <summary>
        ///     Get the syntax node's first ancestor of any of the the specified kinds.
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <param name="syntaxKinds">
        ///     The kinds of node to find.
        /// </param>
        /// <returns>
        ///     The ancestor node, or <c>null</c> if no ancestor of any of the specified kinds was found.
        /// </returns>
        public static SyntaxNode GetFirstParentOfKinds(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            HashSet<SyntaxKind> kinds = new HashSet<SyntaxKind>(syntaxKinds);

            if (kinds.Contains(syntaxNode.Kind))
                return syntaxNode;

            return syntaxNode.AncestorNodes().FirstOrDefault(node => kinds.Contains(node.Kind));
        }

        /// <summary>
        ///     Get the syntax node's nearest containing element or attribute (if any).
        /// </summary>
        /// <param name="syntaxNode">
        ///     The target syntax node.
        /// </param>
        /// <returns>
        ///     The containing element or attribute, or <c>null</c> the syntax node is not a child of an element or attribute.
        /// </returns>
        public static SyntaxNode GetContainingElementOrAttribute(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            return syntaxNode.GetFirstParentOfKinds(
                SyntaxKind.XmlAttribute,
                SyntaxKind.XmlEmptyElement,
                SyntaxKind.XmlElement
            );
        }

        /// <summary>
        ///     Find a child node at the specified position.
        /// </summary>
        /// <param name="syntaxNode">
        ///     The syntax node to be searched.
        /// </param>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <param name="xmlPositions">
        ///     The XML position lookup.
        /// </param>
        /// <returns>
        ///     The syntax node, or <c>null</c> if no node was found at the specified position.
        /// </returns>
        public static SyntaxNode FindNode(this SyntaxNode syntaxNode, Position position, TextPositions xmlPositions)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));

            return SyntaxLocator.FindNode(syntaxNode,
                position: xmlPositions.GetAbsolutePosition(position)
            );
        }

        /// <summary>
        ///     Get a <see cref="Range"/> representing the attribute's span (without quotes) in the XML.
        /// </summary>
        /// <param name="attribute">
        ///     The attribute.
        /// </param>
        /// <param name="xmlPositions">
        ///     The XML position lookup.
        /// </param>
        /// <returns>
        ///     The <see cref="Range"/>.
        /// </returns>
        public static Range GetValueRange(this XmlAttributeSyntax attribute, TextPositions xmlPositions)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));

            Range valueRange = attribute.ValueNode.Span.ToNative(xmlPositions);

            // Trim off leading and trailing quotes.
            return valueRange.Transform(moveStartColumns: 1, moveEndColumns: -1);
        }

        /// <summary>
        ///     Determine whether the element lies before the specified position.
        /// </summary>
        /// <param name="element">
        ///     The element.
        /// </param>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <param name="xmlPositions">
        ///     The XML position lookup.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the element's final closing tag ("/&gt;" or "&gt;") lies after the specified position; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAfterPosition(this XmlElementSyntaxBase element, Position position, TextPositions xmlPositions)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));

            Range endTokenRange;
            if (element is XmlEmptyElementSyntax emptyElement)
                endTokenRange = emptyElement.SlashGreaterThanToken.Span.ToNative(xmlPositions);
            else if (element is XmlElementSyntax elementWithContent)
                endTokenRange = elementWithContent.EndTag.GreaterThanToken.Span.ToNative(xmlPositions);
            else
                throw new ArgumentException($"Unexpected element kind '{element.Kind}'.", nameof(element));

            return position >= endTokenRange;
        }

        /// <summary>
        ///     Determine whether the element lies after the specified position.
        /// </summary>
        /// <param name="element">
        ///     The element.
        /// </param>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <param name="xmlPositions">
        ///     The XML position lookup.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the element's opening tag ("&lt;") lies after the specified position; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAfter(this XmlElementSyntaxBase element, Position position, TextPositions xmlPositions)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));

            Range startTokenRange;
            if (element is XmlEmptyElementSyntax emptyElement)
                startTokenRange = emptyElement.LessThanToken.Span.ToNative(xmlPositions);
            else if (element is XmlElementSyntax elementWithContent)
                startTokenRange = elementWithContent.StartTag.LessThanToken.Span.ToNative(xmlPositions);
            else
                throw new ArgumentException($"Unexpected element kind '{element.Kind}'.", nameof(element));

            return position <= startTokenRange; // We're still before the element when the element "at" the position is the opening tag.
        }
    }
}
