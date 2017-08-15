using System;
using System.Xml;

namespace MSBuildProjectTools.LanguageServer.XmlParser
{
    /// <summary>
    ///     Location information for an XML node.
    /// </summary>
    public abstract class NodeLocation
    {
        /// <summary>
        ///     Create a new <see cref="NodeLocation"/>.
        /// </summary>
        protected NodeLocation()
        {
        }

        /// <summary>
        ///     The text range containing the XML node.
        /// </summary>
        public Range Range { get; internal set; }

        /// <summary>
        ///     The XML node's starting position.
        /// </summary>
        public Position Start => Range?.Start;

        /// <summary>
        ///     The XML node's ending position.
        /// </summary>
        public Position End => Range?.End;

        /// <summary>
        ///     The depth of the nearest surrounding element.
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        ///     The name of the node at the location.
        /// </summary>
        /// <remarks>
        ///     For diagnostic purposes only.
        /// </remarks>
        internal string Name { get; set; }
    }
}
