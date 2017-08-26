using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
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
        /// <param name="node">
        ///     The <see cref="XSNode"/> closest to the position.
        /// </param>
        /// <param name="flags">
        ///     <see cref="XmlPositionFlags"/> value(s) describing the position.
        /// </param>
        public XmlPosition(Position position, int absolutePosition, XSNode node, XmlPositionFlags flags)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (node == null)
                throw new ArgumentNullException(nameof(node));
            
            Position = position;
            AbsolutePosition = absolutePosition;
            Flags = flags;
            Node = node;
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
        ///     Is the position before the nearest <see cref="Node"/>?
        /// </summary>
        public bool IsPositionBeforeNode => Position < Node.Range;

        /// <summary>
        ///     Is the position within the nearest <see cref="Node"/>?
        /// </summary>
        public bool IsPositionWithinNode => Node.Range.Contains(Position);

        /// <summary>
        ///     Is the position after the nearest <see cref="Node"/>?
        /// </summary>
        public bool IsPositionAfterNode => Position > Node.Range;
    }
}
