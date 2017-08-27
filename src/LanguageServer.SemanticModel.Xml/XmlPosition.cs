using Microsoft.Language.Xml;
using System;
using System.Diagnostics;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Information about a position in XML.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplayString()}")]
    public class XmlPosition
    {
        /// <summary>
        ///     Create a new <see cref="XmlPosition"/>.
        /// </summary>
        /// <param name="position">
        ///     The position, in line / column form.
        /// </param>
        /// <param name="absolutePosition">
        ///     The (0-based) absolute position.
        /// </param>
        /// <param name="node">
        ///     The <see cref="XSNode"/> closest to the position.
        /// </param>
        public XmlPosition(Position position, int absolutePosition, XSNode node)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (node == null)
                throw new ArgumentNullException(nameof(node));
            
            Position = position;
            AbsolutePosition = absolutePosition;
            Node = node;

            Flags = ComputeFlags();
        }

        /// <summary>
        ///     The position, in line / column form.
        /// </summary>
        public Position Position { get; }

        /// <summary>
        ///     The (0-based) absolute position.
        /// </summary>
        public int AbsolutePosition { get; }

        /// <summary>
        ///     The <see cref="XSNode"/> closest to the position.
        /// </summary>
        public XSNode Node { get; }
        
        /// <summary>
        ///     The node's parent node (if any).
        /// </summary>
        public XSNode Parent
        {
            get
            {
                switch (Node)
                {
                    case XSElement element:
                    {
                        return element.ParentElement;
                    }
                    case XSAttribute attribute:
                    {
                        return attribute.Element;
                    }
                    case XSElementText textContent:
                    {
                        return textContent.Element;
                    }
                    case XSWhitespace whitespace:
                    {
                        return whitespace.Parent;
                    }
                    default:
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        ///     The next sibling (if any) of the <see cref="XSNode"/> closest to the position.
        /// </summary>
        public XSNode NextSibling => Node?.NextSibling;

        /// <summary>
        ///     The previous sibling (if any) of the <see cref="XSNode"/> closest to the position.
        /// </summary>
        public XSNode PreviousSibling => Node?.PreviousSibling;

        /// <summary>
        ///     <see cref="XmlPositionFlags"/> value(s) describing the position.
        /// </summary>
        public XmlPositionFlags Flags { get; }

        /// <summary>
        ///     Is the position within a name?
        /// </summary>
        public bool IsName => Flags.HasFlag(XmlPositionFlags.Name);

        /// <summary>
        ///     Is the position within an attribute value or element content?
        /// </summary>
        public bool IsValue => Flags.HasFlag(XmlPositionFlags.Value);

        /// <summary>
        ///     Is the position within text content?
        /// </summary>
        public bool IsText => Flags.HasFlag(XmlPositionFlags.Text);

        /// <summary>
        ///     Is the position within whitespace?
        /// </summary>
        public bool IsWhitespace => Flags.HasFlag(XmlPositionFlags.Whitespace);

        /// <summary>
        ///     Is the position within an attribute?
        /// </summary>
        public bool IsAttribute => Flags.HasFlag(XmlPositionFlags.Attribute);

        /// <summary>
        ///     Is the position within an attribute's name?
        /// </summary>
        public bool IsAttributeName => IsAttribute && IsName;

        /// <summary>
        ///     Is the position within an attribute's name?
        /// </summary>
        public bool IsAttributeValue => IsAttribute && IsValue;

        /// <summary>
        ///     Is the position within an element?
        /// </summary>
        public bool IsElement => Flags.HasFlag(XmlPositionFlags.Element);

        /// <summary>
        ///     Is the position within an empty element?
        /// </summary>
        public bool IsEmptyElement => IsElement && Flags.HasFlag(XmlPositionFlags.Empty);

        /// <summary>
        ///     Is the position within element content?
        /// </summary>
        public bool IsElementContent => IsElement && IsValue;

        /// <summary>
        ///     Is the position within an element's opening tag?
        /// </summary>
        public bool IsOpeningTag => Flags.HasFlag(XmlPositionFlags.OpeningTag);

        /// <summary>
        ///     Is the position within an element's closing tag?
        /// </summary>
        public bool IsClosingTag => Flags.HasFlag(XmlPositionFlags.ClosingTag);

        /// <summary>
        ///     Determine <see cref="XmlPositionFlags"/> for the current position.
        /// </summary>
        /// <returns>
        ///     <see cref="XmlPositionFlags"/> describing the position.
        /// </returns>
        XmlPositionFlags ComputeFlags()
        {
            XmlPositionFlags flags = XmlPositionFlags.None;

            switch (Node)
            {
                case XSEmptyElement element:
                {
                    flags |= XmlPositionFlags.Element | XmlPositionFlags.Empty;

                    XmlEmptyElementSyntax syntaxNode = element.ElementNode;

                    TextSpan nameSpan = syntaxNode.NameNode?.Span ?? new TextSpan();
                    if (nameSpan.Contains(AbsolutePosition))
                        flags |= XmlPositionFlags.Name;

                    break;
                }
                case XSElementWithContent elementWithContent:
                {
                    flags |= XmlPositionFlags.Element;

                    XmlElementSyntax syntaxNode = elementWithContent.ElementNode;

                    TextSpan nameSpan = syntaxNode.NameNode?.Span ?? new TextSpan();
                    if (nameSpan.Contains(AbsolutePosition))
                        flags |= XmlPositionFlags.Name;

                    TextSpan startTagSpan = syntaxNode.StartTag?.Span ?? new TextSpan();
                    if (startTagSpan.Contains(AbsolutePosition))
                        flags |= XmlPositionFlags.OpeningTag;

                    TextSpan endTagSpan = syntaxNode.EndTag?.Span ?? new TextSpan();
                    if (endTagSpan.Contains(AbsolutePosition))
                        flags |= XmlPositionFlags.ClosingTag;

                    if (AbsolutePosition >= startTagSpan.End && AbsolutePosition <= endTagSpan.Start)
                        flags |= XmlPositionFlags.Value;

                    break;
                }
                case XSAttribute attribute:
                {
                    flags |= XmlPositionFlags.Attribute;

                    XmlAttributeSyntax syntaxNode = attribute.AttributeNode;

                    TextSpan nameSpan = syntaxNode.NameNode?.Span ?? new TextSpan();
                    if (nameSpan.Contains(AbsolutePosition))
                        flags |= XmlPositionFlags.Name;

                    TextSpan valueSpan = syntaxNode.ValueNode?.Span ?? new TextSpan();
                    if (valueSpan.Contains(AbsolutePosition))
                        flags |= XmlPositionFlags.Value;

                    break;
                }
                case XSElementText text:
                {
                    flags |= XmlPositionFlags.Text | XmlPositionFlags.Element | XmlPositionFlags.Value;

                    break;
                }
                case XSWhitespace whitespace:
                {
                    flags |= XmlPositionFlags.Whitespace | XmlPositionFlags.Element | XmlPositionFlags.Value;

                    break;
                }
            }

            return flags;
        }

        string GetDebuggerDisplayString()
        {
            string nodeDescription = Node.Kind.ToString();
            if (Node is XSElement element)
                nodeDescription += $" '{element.Name}'";
            else if (Node is XSAttribute attribute)
                nodeDescription += $" '{attribute.Name}'";

            return String.Format("XmlPosition({0}) -> {1} @ {2}",
                Position,
                nodeDescription,
                Node.Range
            );
        }
    }
}
