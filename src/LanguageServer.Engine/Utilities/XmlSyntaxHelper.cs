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
        public static IEnumerable<IXmlElement> Descendants(this IEnumerable<IXmlElement> elements)
        {
            foreach (var element in elements)
            {
                yield return element;

                foreach (var descendantElement in element.Descendants())
                {
                    yield return descendantElement;
                }
            }
        }

        public static IEnumerable<IXmlElement> Descendants(this IXmlElement element)
        {
            foreach (var childElement in element.Elements)
            {
                yield return childElement;

                foreach (var descendantElement in childElement.Descendants())
                    yield return descendantElement;
            }
        }

        public static IEnumerable<IXmlElement> Ancestors(this IXmlElement element)
        {
            IXmlElement parent = element.Parent;
            if (parent == null)
                yield break;

            do
                yield return parent;
                while ((parent = parent.Parent) != null);
        }

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
        
        public static TSyntax GetFirstParentOfType<TSyntax>(this SyntaxNode syntaxNode)
        {
            return syntaxNode.AncestorNodes().OfType<TSyntax>().FirstOrDefault();
        }

        public static XmlElementSyntaxBase GetContainingElement(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            if (syntaxNode is XmlElementSyntaxBase element)
                return element;
            
            return syntaxNode.GetFirstParentOfType<XmlElementSyntaxBase>();
        }

        public static XmlAttributeSyntax GetContainingAttribute(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            if (syntaxNode is XmlAttributeSyntax attribute)
                return attribute;
            
            return syntaxNode.GetFirstParentOfType<XmlAttributeSyntax>();
        }

        public static SyntaxNode GetFirstParentOfKind(this SyntaxNode syntaxNode, SyntaxKind syntaxKind)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            if (syntaxNode.Kind == syntaxKind)
                return syntaxNode;

            return syntaxNode.AncestorNodes().FirstOrDefault(node => node.Kind == syntaxKind);
        }

        public static SyntaxNode GetFirstParentOfKinds(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            HashSet<SyntaxKind> kinds = new HashSet<SyntaxKind>(syntaxKinds);

            if (kinds.Contains(syntaxNode.Kind))
                return syntaxNode;

            return syntaxNode.AncestorNodes().FirstOrDefault(node => kinds.Contains(node.Kind));
        }

        public static SyntaxNode GetContainingAttributeOrElement(this SyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            return syntaxNode.GetFirstParentOfKinds(
                SyntaxKind.XmlAttribute,
                SyntaxKind.XmlEmptyElement,
                SyntaxKind.XmlElement
            );
        }

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
    }
}
