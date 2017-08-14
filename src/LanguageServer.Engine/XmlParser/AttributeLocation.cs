using System;
using System.Xml;

namespace MSBuildProjectTools.LanguageServer.XmlParser
{
    /// <summary>
    ///     Location information for an XML attribute.
    /// </summary>
    public class AttributeLocation
        : Location
    {
        /// <summary>
        ///     The starting position of the attribute name.
        /// </summary>
        public Position NameStart { get; internal set; }

        /// <summary>
        ///     The ending position of the attribute name.
        /// </summary>
        public Position NameEnd { get; internal set; }

        /// <summary>
        ///     The starting position of the attribute value.
        /// </summary>
        public Position ValueStart { get; internal set; }

        /// <summary>
        ///     The ending position of the attribute value.
        /// </summary>
        public Position ValueEnd { get; internal set; }

        /// <summary>
        ///     Create an <see cref="AttributeLocation"/>, calculating values as required.
        /// </summary>
        /// <param name="start">
        ///     The attribute's starting position.
        /// </param>
        /// <param name="name">
        ///     The attribute name.
        /// </param>
        /// <param name="value">
        ///     The attribute value.
        /// </param>
        /// <returns>
        ///     The new <see cref="AttributeLocation"/>.
        /// </returns>
        public static AttributeLocation Create(Position start, string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new AttributeLocation
            {
                Start = start,
                End = start.Move(
                    columnCount: name.Length + 2 /* =" */ + value.Length + 1 /* " */
                ),
                NameStart = start,
                NameEnd = start.Move(
                    columnCount: name.Length
                ),
                ValueStart = start.Move(
                    columnCount: name.Length + 2 /* =" */
                ),
                ValueEnd = start.Move(
                    columnCount: name.Length + 2 /* =" */ + value.Length
                )
            };
        }
    }
}
