using System;
using System.Xml;

namespace MSBuildProjectTools.LanguageServer.XmlParser
{
    /// <summary>
    ///     Location information for an XML node.
    /// </summary>
    public abstract class Location
    {
        /// <summary>
        ///     Create a new <see cref="Location"/>.
        /// </summary>
        protected Location()
        {
        }

        /// <summary>
        ///     The starting position of the XML node.
        /// </summary>
        public Position Start { get; internal set; }

        /// <summary>
        ///     The ending position of the XML node.
        /// </summary>
        public Position End { get; internal set; }

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

        /// <summary>
        ///     Determine whether the location's range contains the specified position.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     <c>true></c>, if the location's start / end range contains the target position; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            
            return position >= Start && position <= End;
        }
    }
}
