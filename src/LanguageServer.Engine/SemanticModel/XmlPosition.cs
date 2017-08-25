using Microsoft.Language.Xml;
using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using System.Linq;
    /// <summary>
    ///     Information about a position in XML.
    /// </summary>
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
        /// <param name="flags">
        ///     <see cref="XmlPositionFlags"/> value(s) describing the position.
        /// </param>
        /// <param name="node">
        ///     The <see cref="SyntaxNode"/> closest to the position.
        /// </param>
        /// <param name="elementOrAttribute">
        ///     The <paramref name="node"/>'s containing element or attribute.
        /// </param>
        public XmlPosition(Position position, int absolutePosition, XmlPositionFlags flags, SyntaxNode node, SyntaxNode elementOrAttribute)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (node == null)
                throw new ArgumentNullException(nameof(node));
            
            Position = position;
            AbsolutePosition = absolutePosition;
            Flags = flags;
            Node = node;
            ElementOrAttribute = elementOrAttribute ?? node;
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
        ///     <see cref="XmlPositionFlags"/> value(s) describing the position.
        /// </summary>
        public XmlPositionFlags Flags { get; }

        /// <summary>
        ///     The <see cref="SyntaxNode"/> closest to the position.
        /// </summary>
        public SyntaxNode Node { get; }

        /// <summary>
        ///     The <see cref="Node"/>'s containing element or attribute.
        /// </summary>
        public SyntaxNode ElementOrAttribute { get; }

        /// <summary>
        ///     Is the position within an element?
        /// </summary>
        public bool IsElement => Flags.HasFlag(XmlPositionFlags.Element);

        /// <summary>
        ///     Is the position within element content?
        /// </summary>
        public bool IsElementContent => Flags.HasFlag(XmlPositionFlags.ElementContent);

        /// <summary>
        ///     Is the position within an element attributes?
        /// </summary>
        public bool IsElementAttributes => Flags.HasFlag(XmlPositionFlags.ElementAttributes);

        /// <summary>
        ///     Is the position within an attribute?
        /// </summary>
        public bool IsAttribute => Flags.HasFlag(XmlPositionFlags.Attribute);

        /// <summary>
        ///     Is the position within a name?
        /// </summary>
        public bool IsName => Flags.HasFlag(XmlPositionFlags.Name);

        /// <summary>
        ///     Is the position within an element's opening tag?
        /// </summary>
        public bool IsOpeningTag => Flags.HasFlag(XmlPositionFlags.OpeningTag);

        /// <summary>
        ///     Is the position within an element's closing tag?
        /// </summary>
        public bool IsClosingTag => Flags.HasFlag(XmlPositionFlags.ClosingTag);

        /// <summary>
        ///     Is the position before the nearest <see cref="Node"/>?
        /// </summary>
        public bool IsPositionBeforeNode => AbsolutePosition < Node.Span.Start;

        /// <summary>
        ///     Is the position within the nearest <see cref="Node"/>?
        /// </summary>
        public bool IsPositionWithinNode => Node.Span.Contains(AbsolutePosition);

        /// <summary>
        ///     Is the position after the nearest <see cref="Node"/>?
        /// </summary>
        public bool IsPositionAfterNode => AbsolutePosition > Node.Span.End;

        /// <summary>
        ///     Is the position before the nearest <see cref="ElementOrAttribute"/>?
        /// </summary>
        public bool IsPositionBeforeElementOrAttribute => AbsolutePosition < ElementOrAttribute.Span.Start;

        /// <summary>
        ///     Is the position within the nearest <see cref="ElementOrAttribute"/>?
        /// </summary>
        public bool IsPositionWithinElementOrAttribute => ElementOrAttribute.Span.Contains(AbsolutePosition);

        /// <summary>
        ///     Is the position after the nearest <see cref="ElementOrAttribute"/>?
        /// </summary>
        public bool IsPositionAfterElementOrAttribute => AbsolutePosition > ElementOrAttribute.Span.End;
    }
}
