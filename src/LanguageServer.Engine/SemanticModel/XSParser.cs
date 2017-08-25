using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using Utilities;

    /// <summary>
    ///     Parses the syntax model to derive a semantic model.
    /// </summary>
    public static class XSParser
    {
        /// <summary>
        ///     Parse the syntax model to derive a semantic model.
        /// </summary>
        /// <param name="node">
        ///     The <see cref="SyntaxNode"/> to parse.
        /// </param>
        /// <param name="xmlPositions">
        ///     The lookup for document positions.
        /// </param>
        /// <returns>
        ///     A list of <see cref="XSNode"/>s, sorted by <see cref="XSNode.Range"/>.
        /// </returns>
        public static List<XSNode> GetSemanticModel(this XmlNodeSyntax node, TextPositions xmlPositions)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));

            XSParserVisitor parserVisitor = new XSParserVisitor(xmlPositions);
            parserVisitor.Visit(node);

            parserVisitor.DiscoveredNodes.Sort(
                (node1, node2) => node1.Range.CompareTo(node2.Range)
            );
            
            return parserVisitor.DiscoveredNodes;
        }

        /// <summary>
        ///     A syntax visitor that extracts a semantic model (in the form of <see cref="XSNode"/>s) from syntax nodes.
        /// </summary>
        class XSParserVisitor
            : SyntaxVisitor
        {
            /// <summary>
            ///     The stack of <see cref="XSElement"/>s being processed.
            /// </summary>
            readonly Stack<XSElement> _elementStack = new Stack<XSElement>();

            /// <summary>
            ///     Lookup for document text positions.
            /// </summary>
            readonly TextPositions _textPositions;

            /// <summary>
            ///     Create a new <see cref="XSParserVisitor"/>.
            /// </summary>
            /// <param name="textPositions">
            ///     Lookup for document text positions.
            /// </param>
            public XSParserVisitor(TextPositions textPositions)
            {
                if (textPositions == null)
                    throw new ArgumentNullException(nameof(textPositions));

                _textPositions = textPositions;
            }

            /// <summary>
            ///     <see cref="XSNode"/>s discovered in the XML.
            /// </summary>
            public List<XSNode> DiscoveredNodes { get; } = new List<XSNode>();

            /// <summary>
            ///     Is there an element currently being processed?
            /// </summary>
            bool HaveCurrentElement => _elementStack.Count > 0;

            /// <summary>
            ///     The element (if any) being processed.
            /// </summary>
            XSElement CurrentElement => HaveCurrentElement ? _elementStack.Peek() : null;

            /// <summary>
            ///     Visit an <see cref="XmlElementSyntax"/>.
            /// </summary>
            /// <param name="element">
            ///     The <see cref="XmlElementSyntax"/>.
            /// </param>
            /// <returns>
            ///     The <see cref="XmlElementSyntax"/> (unchanged).
            /// </returns>
            public override SyntaxNode VisitXmlElement(XmlElementSyntax element)
            {
                Range elementRange = element.Span.ToNative(_textPositions);
                Range openingTagRange = element.StartTag?.Span.ToNative(_textPositions) ?? elementRange;
                Range closingTagRange = element.EndTag?.Span.ToNative(_textPositions) ?? openingTagRange;
                Range contentRange = new Range(
                    start: openingTagRange.End,
                    end: closingTagRange.Start
                );

                XSElement xsElement;
                if (String.IsNullOrWhiteSpace(element.Name) || openingTagRange == elementRange || contentRange == elementRange || closingTagRange == elementRange)
                    xsElement = new XSInvalidElement(element, elementRange, hasContent: true);
                else
                    xsElement = new XSElementWithContent(element, elementRange, openingTagRange, contentRange, closingTagRange);

                DiscoveredNodes.Add(xsElement);

                PushElement(xsElement);
                SyntaxNode result = base.VisitXmlElement(element);
                PopElement();

                return result;
            }

            /// <summary>
            ///     Visit an <see cref="XmlEmptyElementSyntax"/>.
            /// </summary>
            /// <param name="emptyElement">
            ///     The <see cref="XmlEmptyElementSyntax"/>.
            /// </param>
            /// <returns>
            ///     The <see cref="XmlEmptyElementSyntax"/> (unchanged).
            /// </returns>
            public override SyntaxNode VisitXmlEmptyElement(XmlEmptyElementSyntax emptyElement)
            {
                Range elementRange = emptyElement.Span.ToNative(_textPositions);
                XSElement xsElement;

                if (String.IsNullOrWhiteSpace(emptyElement.Name))
                    xsElement = new XSInvalidElement(emptyElement, elementRange, hasContent: false);
                else
                    xsElement = new XSEmptyElement(emptyElement, elementRange);

                DiscoveredNodes.Add(xsElement);

                PushElement(xsElement);
                SyntaxNode result = base.VisitXmlEmptyElement(emptyElement);
                PopElement();

                return result;
            }

            /// <summary>
            ///     Visit an <see cref="XmlAttributeSyntax"/>.
            /// </summary>
            /// <param name="attribute">
            ///     The <see cref="XmlAttributeSyntax"/>.
            /// </param>
            /// <returns>
            ///     The <see cref="XmlAttributeSyntax"/> (unchanged).
            /// </returns>
            public override SyntaxNode VisitXmlAttribute(XmlAttributeSyntax attribute)
            {
                if (!HaveCurrentElement)
                    return base.VisitXmlAttribute(attribute);

                Range attributeRange = attribute.Span.ToNative(_textPositions);
                Range nameRange = attribute.NameNode?.Span.ToNative(_textPositions) ?? attributeRange;
                Range valueRange = attribute.ValueNode?.Span.ToNative(_textPositions) ?? nameRange;

                XSAttribute xsAttribute;
                if (String.IsNullOrWhiteSpace(attribute.Name) || nameRange == attributeRange || valueRange == attributeRange)
                    xsAttribute = new XSInvalidAttribute(attribute, attributeRange, nameRange, valueRange, CurrentElement);
                else
                    xsAttribute = new XSAttribute(attribute, attributeRange, nameRange, valueRange, CurrentElement);

                CurrentElement.AddAttribute(xsAttribute);
                DiscoveredNodes.Add(xsAttribute);

                return base.VisitXmlAttribute(attribute);
            }

            /// <summary>
            ///     Visit an <see cref="XmlTextSyntax"/>.
            /// </summary>
            /// <param name="text">
            ///     The <see cref="XmlTextSyntax"/>.
            /// </param>
            /// <returns>
            ///     The <see cref="XmlTextSyntax"/> (unchanged).
            /// </returns>
            public override SyntaxNode VisitXmlText(XmlTextSyntax text)
            {
                Range textRange = text.Span.ToNative(_textPositions);
                if (CurrentElement == null || !CurrentElement.Range.Contains(textRange))
                    return base.VisitXmlText(text);

                DiscoveredNodes.Add(
                    new XSElementText(text, textRange, CurrentElement)
                );

                return base.VisitXmlText(text);
            }

            /// <summary>
            ///     Push an <see cref="XSElement"/> onto the stack.
            /// </summary>
            /// <param name="element">
            ///     The <see cref="XSElement"/> being processed.
            /// </param>
            /// <returns>
            ///     The <see cref="XSElement"/>.
            /// </returns>
            XSElement PushElement(XSElement element)
            {
                if (element == null)
                    throw new ArgumentNullException(nameof(element));

                _elementStack.Push(element);

                return element;
            }

            /// <summary>
            ///     Pop an element from the stack.
            /// </summary>
            void PopElement() => _elementStack.Pop();
        }
    }
}
