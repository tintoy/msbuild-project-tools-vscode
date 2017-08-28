using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
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
        ///     Does the location represent an attribute's value?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="attribute">
        ///     Receives the attribute whose value is represented by the location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an attribute's name; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAttributeValue(this XmlLocation location, out XSAttribute attribute)
        {
            if (location.IsAttributeValue())
            {
                attribute = (XSAttribute)location.Node;

                return true;
            }

            attribute = null;

            return false;
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

            if (location.IsElement() && !location.IsElementContent())
            {
                element = (XSElement)location.Node;

                return true;
            }
            else
            {
                element = null;

                return false;
            }
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
            else
            {
                emptyElement = (XSEmptyElement)location.Node;

                return true;
            }
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
        ///     Does the location represent an element's textual content?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element's textual content; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElementText(this XmlLocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return location.IsElement() && location.IsText();
        }

        /// <summary>
        ///     Does the location represent an element's textual content?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="text">
        ///     Receives the <see cref="XSElementText"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element's textual content; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsElementText(this XmlLocation location, out XSElementText text)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (location.IsElementText())
            {
                text = (XSElementText)location.Node;

                return true;
            }
            else
            {
                text = null;

                return false;
            }
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

        /// <summary>
        ///     Does the location represent a place where an element can be created or replaced by a completion?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="replaceElement">
        ///     The element (if any) that will be replaced by the completion.
        /// </param>
        /// <param name="asChildOfElementNamed">
        ///     If specified, the location's parent element must have the specified name.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an element that can be replaced by completion; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     We can replace "&lt;&gt;" and "&lt;&lt;Element /&gt;".
        /// </remarks>
        public static bool CanCompleteElement(this XmlLocation location, out XSElement replaceElement, string asChildOfElementNamed = null)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            replaceElement = null;
            if (location.IsWhitespace())
                return true;

            XSElement element;
            if (!location.IsElement(out element))
                return false;

            if (element.IsValid)
            {
                // If the element is valid, then check if we have an invalid parent (e.g. "<<Foo />" yields invalid element "" with child element "Foo").

                if (element.ParentElement.IsValid)
                    return false;

                if (element.ParentElement.Start.LineNumber != location.Node.Start.LineNumber)
                    return false;

                if (location.Node.Start.ColumnNumber - element.ParentElement.Start.ColumnNumber == 1)
                    element = element.ParentElement;
            }
            else if (element.Start.LineNumber != element.End.LineNumber || element.End.ColumnNumber - element.Start.ColumnNumber != 2)
                return false; // Not "<>", which is what VSCode inserts when you're not directly to the left of an element and type "<".

            if (asChildOfElementNamed != null && element.ParentElement?.Name != asChildOfElementNamed)
                return false;

            replaceElement = element;

            return true;
        }

        /// <summary>
        ///     Does the location represent a place where an attribute value can be created or replaced by a completion?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="targetAttribute">
        ///     The attribute (if any) whose value will be replaced by the completion.
        /// </param>
        /// <param name="onElementNamed">
        ///     If specified, attribute's element must have the specified name.
        /// </param>
        /// <param name="forAttributeNamed">
        ///     If specified, the attribute must have one of the specified names.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an attribute whose value can be replaced by a completion; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanCompleteAttributeValue(this XmlLocation location, out XSAttribute targetAttribute, string onElementNamed = null, params string[] forAttributeNamed)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));
            
            targetAttribute = null;

            XSAttribute attribute;
            if (!location.IsAttributeValue(out attribute))
                return false;

            if (onElementNamed != null && attribute.Element.Name != onElementNamed)
                return false;

            if (forAttributeNamed.Length > 0 && Array.IndexOf(forAttributeNamed, attribute.Name) == -1)
                return false;

            targetAttribute = attribute;

            return true;
        }
    }
}
