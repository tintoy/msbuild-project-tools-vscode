using Microsoft.Language.Xml;
using System;
using System.Diagnostics;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Information about a position in XML.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplayString()}")]
    public class XmlLocation
    {
        /// <summary>
        ///     Create a new <see cref="XmlLocation"/>.
        /// </summary>
        /// <param name="position">
        ///     The location's position, in line / column form.
        /// </param>
        /// <param name="absolutePosition">
        ///     The location's (0-based) absolute position.
        /// </param>
        /// <param name="node">
        ///     The <see cref="XSNode"/> closest to the location's position.
        /// </param>
        /// <param name="flags">
        ///     <see cref="XmlLocationFlags"/> describing the location.
        /// </param>
        public XmlLocation(Position position, int absolutePosition, XSNode node, XmlLocationFlags flags)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (node == null)
                throw new ArgumentNullException(nameof(node));
            
            Position = position;
            AbsolutePosition = absolutePosition;
            Node = node;
            Flags = flags;
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
        ///     <see cref="XmlLocationFlags"/> value(s) describing the position.
        /// </summary>
        public XmlLocationFlags Flags { get; }

        /// <summary>
        ///     Get a string representation of the <see cref="XmlLocation"/> for display in the debugger.
        /// </summary>
        /// <returns>
        ///     The debugger display string.
        /// </returns>
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

    /// <summary>
    ///     Extension methods for <see cref="XmlLocation"/>.
    /// </summary>
    public static class XmlLocationExtensions
    {
        /// <summary>
        ///     Does the location represent a name?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element or attribute name; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsName(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.Flags.HasFlag(XmlLocationFlags.Name);
        }

        /// <summary>
        ///     Does the location represent a value?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents element content (text / whitespace) or an attribute value; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValue(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.Flags.HasFlag(XmlLocationFlags.Value);
        }

        /// <summary>
        ///     Does the location represent text?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents text content within an element; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsText(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.Flags.HasFlag(XmlLocationFlags.Text);
        }

        /// <summary>
        ///     Does the location represent whitespace?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents whitespace within element content; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWhitespace(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.Flags.HasFlag(XmlLocationFlags.Whitespace);
        }

        /// <summary>
        ///     Does the location represent an attribute?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an attribute; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAttribute(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.Flags.HasFlag(XmlLocationFlags.Attribute);
        }

        /// <summary>
        ///     Does the location represent an attribute?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="attribute">
        ///     Receives the <see cref="XSAttribute"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an attribute; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAttribute(this XmlLocation location, out XSAttribute attribute)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (!location.IsAttribute())
            {
                attribute = null;

                return false;
            }

            attribute = (XSAttribute)location.Node;

            return true;
        }

        /// <summary>
        ///     Does the location represent an attribute's name?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an attribute's name; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAttributeName(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsAttribute() && location.IsName();
        }

        /// <summary>
        ///     Does the location represent an attribute's value?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an attribute's name; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAttributeValue(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsAttribute() && location.IsValue();
        }

        /// <summary>
        ///     Does the location represent an element?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElement(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.Flags.HasFlag(XmlLocationFlags.Element);
        }

        /// <summary>
        ///     Does the location represent an element?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="element">
        ///     Receives the <see cref="XSElement"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElement(this XmlLocation location, out XSElement element)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (!location.IsElement() || location.IsElementContent())
            {
                element = null;

                return false;
            }

            element = (XSElement)location.Node;

            return true;
        }

        /// <summary>
        ///     Does the location represent an empty element?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an empty element; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmptyElement(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsElement() && location.Flags.HasFlag(XmlLocationFlags.Empty);
        }

        /// <summary>
        ///     Does the location represent an empty element?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="emptyElement">
        ///     Receives the <see cref="XSEmptyElement"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an empty element; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmptyElement(this XmlLocation location, out XSEmptyElement emptyElement)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (!location.IsEmptyElement())
            {
                emptyElement = null;

                return false;
            }

            emptyElement = (XSEmptyElement)location.Node;

            return true;
        }

        /// <summary>
        ///     Does the location represent an element content (i.e. text or whitespace)?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents element content; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElementContent(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsElement() && location.IsValue();
        }

        /// <summary>
        ///     Does the location represent an element's opening tag?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element's opening tag; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElementOpeningTag(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsElement() && location.Flags.HasFlag(XmlLocationFlags.OpeningTag);
        }

        /// <summary>
        ///     Does the location represent an element's closing tag?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element's closing tag; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElementClosingTag(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsElement() && location.Flags.HasFlag(XmlLocationFlags.ClosingTag);
        }

        /// <summary>
        ///     Does the location represent an element or an attribute?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element or attribute; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElementOrAttribute(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsElement() || location.IsAttribute();
        }
    }
}
