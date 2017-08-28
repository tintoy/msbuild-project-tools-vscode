using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Flags describing a location in XML.
    /// </summary>
    [Flags]
    public enum XmlLocationFlags
    {
        /// <summary>
        ///     No flags.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Position is on an element.
        /// </summary>
        Element = 1,

        /// <summary>
        ///     Position is on the opening tag of an element.
        /// </summary>
        OpeningTag = 2,

        /// <summary>
        ///     Position is on the closing tag of an element.
        /// </summary>
        ClosingTag = 4,

        /// <summary>
        ///     Position is on an attribute.
        /// </summary>
        Attribute = 8,

        /// <summary>
        ///     Position is within an element's attributes range (but not on a specific attribute).
        /// </summary>
        Attributes = 16,

        /// <summary>
        ///     Position is on a name.
        /// </summary>
        Name = 32,

        /// <summary>
        ///     Position is on element content / attribute value.
        /// </summary>
        Value = 64,

        /// <summary>
        ///     Position is on text.
        /// </summary>
        Text = 128,

        /// <summary>
        ///     Position is on whitespace.
        /// </summary>
        Whitespace = 256,

        /// <summary>
        ///     Element or attribute has no content.
        /// </summary>
        Empty = 1024,

        /// <summary>
        ///     Node at location does not represent valid XML.
        /// </summary>
        Invalid = 2048
    }
}
