using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using Utilities;

    // TODO: Consider defining and capturing XSElementEndTag.

    /// <summary>
    ///     Parses the syntax model to derive a semantic model.
    /// </summary>
    public static class XSParser
    {
        /// <summary>
        ///     Parse the syntax model to derive a semantic model.
        /// </summary>
        /// <param name="document">
        ///     The <see cref="XmlDocumentSyntax"/> to parse.
        /// </param>
        /// <param name="xmlPositions">
        ///     The lookup for document positions.
        /// </param>
        /// <returns>
        ///     A list of <see cref="XSNode"/>s, sorted by <see cref="XSNode.Range"/>.
        /// </returns>
        public static List<XSNode> GetSemanticModel(this XmlDocumentSyntax document, TextPositions xmlPositions)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));

            XSParserVisitor parserVisitor = new XSParserVisitor(xmlPositions);
            parserVisitor.Visit(document);
            parserVisitor.FinaliseModel();

            return parserVisitor.DiscoveredNodes;
        }

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
            parserVisitor.FinaliseModel();

            return parserVisitor.DiscoveredNodes;
        }

        /// <summary>
        ///     A syntax visitor that extracts a semantic model (in the form of <see cref="XSNode"/>s) from syntax nodes.
        /// </summary>
        class XSParserVisitor
            : SyntaxVisitor
        {
            /// <summary>
            ///     Spans for whitespace that has already been processed.
            /// </summary>
            readonly SortedSet<TextSpan> _whitespaceSpans = new SortedSet<TextSpan>();

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
            ///     Perform final processing of discovered nodes.
            /// </summary>
            public void FinaliseModel()
            {
                ComputeWhitespace();

                XSNode[] sortedNodes =
                    DiscoveredNodes
                        .OrderBy(discoveredNode => discoveredNode.Range.Start)
                        .ThenBy(discoveredNode => discoveredNode.Range.End)
                        .ToArray();
                DiscoveredNodes.Clear();
                DiscoveredNodes.AddRange(sortedNodes);

                ConnectSiblings();
            }

            /// <summary>
            ///     Find the spaces between elements and use that to infer whitespace.
            /// </summary>
            void ComputeWhitespace()
            {
                // TODO: Merge contiguous whitespace.

                var discoveredElements = DiscoveredNodes.OfType<XSElementWithContent>()
                        .OrderBy(discoveredNode => discoveredNode.Range.Start)
                        .ThenBy(discoveredNode => discoveredNode.Range.End);

                foreach (XSElementWithContent element in discoveredElements)
                {
                    int startOfNextNode, endOfNode, whitespaceLength;
                    XSWhitespace whitespace;

                    endOfNode = element.ElementNode.StartTag.Span.End;
                    for (int contentIndex = 0; contentIndex < element.Content.Count; contentIndex++)
                    {
                        if (element.Content[contentIndex] is XSElementText text)
                        {
                            startOfNextNode = _textPositions.GetAbsolutePosition(text.Range.Start);

                            whitespaceLength = startOfNextNode - endOfNode;
                            if (whitespaceLength > 0)
                            {
                                whitespace = new XSWhitespace(
                                    range: new Range(
                                        start: _textPositions.GetPosition(endOfNode),
                                        end: _textPositions.GetPosition(startOfNextNode)
                                    ),
                                    parent: element
                                );
                                element.Content.Insert(contentIndex, whitespace);
                                DiscoveredNodes.Add(whitespace);
                            }

                            endOfNode = _textPositions.GetAbsolutePosition(text.Range.End);
                        }

                        if (element.Content[contentIndex] is XSElement childElement)
                        {
                            startOfNextNode = childElement.ElementNode.Span.Start;

                            whitespaceLength = startOfNextNode - endOfNode;
                            if (whitespaceLength > 0)
                            {
                                whitespace = new XSWhitespace(
                                    range: new Range(
                                        start: _textPositions.GetPosition(endOfNode),
                                        end: _textPositions.GetPosition(startOfNextNode)
                                    ),
                                    parent: element
                                );
                                element.Content.Insert(contentIndex, whitespace);
                                DiscoveredNodes.Add(whitespace);
                            }

                            endOfNode = childElement.ElementNode.Span.End;
                        }
                    }

                    // Any trailing whitespace before the closing tag?
                    startOfNextNode = element.ElementNode.EndTag.Span.Start;
                    whitespaceLength = startOfNextNode - endOfNode;
                    if (whitespaceLength > 0)
                    {
                        whitespace = new XSWhitespace(
                            range: new Range(
                                start: _textPositions.GetPosition(endOfNode),
                                end: _textPositions.GetPosition(startOfNextNode)
                            ),
                            parent: element
                        );
                        element.Content.Add(whitespace);
                        DiscoveredNodes.Add(whitespace);
                    }
                }
            }

            /// <summary>
            ///     Connect sibling nodes.
            /// </summary>
            void ConnectSiblings()
            {
                // TODO: Move attribute sibiling connections here, too (currently done when adding the attributes).

                var discoveredElements = DiscoveredNodes.OfType<XSElement>();
                foreach (XSElement element in discoveredElements)
                {
                    // Join up sibling attributes.
                    XSNode previousSibling = null;
                    foreach (XSAttribute nextSibling in element.Attributes)
                    {
                        nextSibling.PreviousSibling = previousSibling;
                        if (previousSibling != null)
                            previousSibling.NextSibling = nextSibling;

                        previousSibling = nextSibling;
                    }

                    previousSibling = null;

                    // Join up sibling content nodes.
                    foreach (XSNode nextSibling in element.Content)
                    {
                        nextSibling.PreviousSibling = previousSibling;
                        if (previousSibling != null)
                            previousSibling.NextSibling = nextSibling;

                        previousSibling = nextSibling;
                    }
                }
            }

            /// <summary>
            ///     Visit an <see cref="XmlDocumentSyntax"/>.
            /// </summary>
            /// <param name="document">
            ///     The <see cref="XmlDocumentSyntax"/>.
            /// </param>
            /// <returns>
            ///     The <see cref="XmlDocumentSyntax"/> (unchanged).
            /// </returns>
            public override SyntaxNode VisitXmlDocument(XmlDocumentSyntax document)
            {
                foreach (XmlElementSyntaxBase element in document.Elements.OfType<XmlElementSyntaxBase>())
                    Visit(element);

                return document;
            }

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
                Range closingTagRange = element.EndTag?.Span.ToNative(_textPositions) ?? elementRange;
                Range contentRange;
                if (openingTagRange.End <= closingTagRange.Start)
                {
                    contentRange = new Range(
                        start: openingTagRange.End,
                        end: closingTagRange.Start
                    );
                }
                else
                    contentRange = elementRange;

                XSElement xsElement;
                if (String.IsNullOrWhiteSpace(element.Name) || openingTagRange == elementRange || contentRange == elementRange || closingTagRange == elementRange)
                    xsElement = new XSInvalidElement(element, elementRange, parent: CurrentElement, hasContent: true);
                else
                    xsElement = new XSElementWithContent(element, elementRange, openingTagRange, contentRange, closingTagRange, parent: CurrentElement);

                if (xsElement.ParentElement is XSElementWithContent parentElement)
                    parentElement.Content.Add(xsElement);

                PushElement(xsElement);

                foreach (XmlAttributeSyntax attribute in element.AsSyntaxElement.Attributes)
                    Visit(attribute);

                if (element.Content is SyntaxList childElements)
                {
                    foreach (XmlElementSyntaxBase childElement in childElements.ChildNodes.OfType<XmlElementSyntaxBase>())
                        Visit(childElement);
                }
                else if (element.Content is XmlElementSyntaxBase singleChildElement)
                    Visit(singleChildElement);
                else if (element.Content is XmlTextSyntax text)
                    VisitXmlText(text);

                PopElement();

                return element;
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
                    xsElement = new XSInvalidElement(emptyElement, elementRange, parent: CurrentElement, hasContent: false);
                else
                    xsElement = new XSEmptyElement(emptyElement, elementRange, parent: CurrentElement);

                if (xsElement.ParentElement is XSElementWithContent parentElement)
                    parentElement.Content.Add(xsElement);

                PushElement(xsElement);

                foreach (XmlAttributeSyntax attribute in emptyElement.AsSyntaxElement.Attributes)
                    Visit(attribute);

                PopElement();

                return emptyElement;
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
                    return attribute;

                Range attributeRange = attribute.Span.ToNative(_textPositions);
                Range nameRange = attribute.NameNode?.Span.ToNative(_textPositions) ?? attributeRange;
                Range valueRange = attribute.ValueNode?.Span.ToNative(_textPositions) ?? nameRange;

                XSAttribute xsAttribute;
                if (String.IsNullOrWhiteSpace(attribute.Name) || nameRange == attributeRange || valueRange == attributeRange)
                    xsAttribute = new XSInvalidAttribute(attribute, CurrentElement, attributeRange, nameRange, valueRange);
                else
                    xsAttribute = new XSAttribute(attribute, CurrentElement, attributeRange, nameRange, valueRange);

                CurrentElement.Attributes.Add(xsAttribute);
                DiscoveredNodes.Add(xsAttribute);

                return attribute;
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
                    return text;

                XSElementText elementText = new XSElementText(text, textRange, CurrentElement);
                CurrentElement.Content.Add(elementText);

                DiscoveredNodes.Add(elementText);

                return text;
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

                DiscoveredNodes.Add(element);

                return element;
            }

            /// <summary>
            ///     Pop an element from the stack.
            /// </summary>
            void PopElement()
            {
                _elementStack.Pop();
            }
        }
    }
}
