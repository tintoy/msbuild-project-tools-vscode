using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.XmlParser
{
    /// <summary>
    ///     A parser for XML that annotates the XML with location information.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        ///     Parse the specified XML.
        /// </summary>
        /// <param name="xml">
        ///     A string containing the XML to parse.
        /// </param>
        /// <returns>
        ///     The parsed XML, as an <see cref="XDocument"/>.
        /// </returns>
        public static XDocument Parse(string xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            using (StringReader reader = new StringReader(xml))
            {
                return Load(reader);
            }
        }

        /// <summary>
        ///     Load an <see cref="XDocument"/> with <see cref="NodeLocation"/> annotations.
        /// </summary>
        /// <param name="filePath">
        ///     The path of the file containing the XML.
        /// </param>
        /// <returns>
        ///     The <see cref="XDocument"/>.
        /// </returns>
        public static XDocument Load(string filePath)
        {
            using (StreamReader reader = File.OpenText(filePath))
            {
                return Load(reader);
            }
        }

        /// <summary>
        ///     Load an <see cref="XDocument"/> with <see cref="NodeLocation"/> annotations.
        /// </summary>
        /// <param name="stream">
        ///     The <see cref="Stream"/> to read from.
        /// </param>
        /// <returns>
        ///     The <see cref="XDocument"/>.
        /// </returns>
        public static XDocument Load(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return Load(reader);
            }
        }

        /// <summary>
        ///     Load an <see cref="XDocument"/> with <see cref="NodeLocation"/> annotations.
        /// </summary>
        /// <param name="textReader">
        ///     The <see cref="TextReader"/> to read from.
        /// </param>
        /// <returns>
        ///     The <see cref="XDocument"/>.
        /// </returns>
        public static XDocument Load(TextReader textReader)
        {
            XDocument document;
            using (LocatingXmlTextReader xmlReader = new LocatingXmlTextReader(textReader))
            {
                document = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
                AddLocations(document.Root,
                    new Queue<NodeLocation>(xmlReader.Locations)
                );
            }

            return document;
        }

        /// <summary>
        ///     Recursively add <see cref="NodeLocation"/> annotations to the specified <see cref="XElement"/>.
        /// </summary>
        /// <param name="element">
        ///     The <see cref="XElement"/>.
        /// </param>
        /// <param name="locations">
        ///     A queue containing location information for elements and attributes (document-node order).
        /// </param>
        static void AddLocations(XElement element, Queue<NodeLocation> locations)
        {
            element.AddAnnotation(
                (ElementLocation)locations.Dequeue()
            );
            foreach (XAttribute attribute in element.Attributes())
            {
                attribute.AddAnnotation(
                    (AttributeLocation)locations.Dequeue()
                );
            }
            foreach (XElement childElement in element.Elements())
                AddLocations(childElement, locations);
        }
    }
}
