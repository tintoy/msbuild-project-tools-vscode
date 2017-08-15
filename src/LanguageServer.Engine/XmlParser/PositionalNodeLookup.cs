using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.XmlParser
{
    /// <summary>
    ///     Handles lookups of XML objects in a document by textual position.
    /// </summary>
    public class PositionalObjectLookup
    {
        /// <summary>
        ///     The ranges for all XML objects in the document with positional annotations.
        /// </summary>
        /// <remarks>
        ///     Sorted by range comparison (effectively, this means document order).
        /// </remarks>
        readonly List<Range> _objectRanges = new List<Range>();

        /// <summary>
        ///     All XML objects in the document with positional annotations, keyed by starting position.
        /// </summary>
        /// <remarks>
        ///     Sorted by range comparison.
        /// </remarks>
        readonly SortedDictionary<Position, XObject> _objectsByStartPosition = new SortedDictionary<Position, XObject>();
        
        /// <summary>
        ///     The XML document.
        /// </summary>
        readonly XDocument _document;
        
        /// <summary>
        ///     Create a new <see cref="PositionalObjectLookup"/> for an XML document.
        /// </summary>
        /// <param name="document">
        ///     The XML document.
        /// </param>
        public PositionalObjectLookup(XDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _document = document;
            foreach (XElement element in document.Descendants())
            {
                ElementLocation elementLocation = element.Annotation<ElementLocation>();
                if (elementLocation == null)
                    continue;

                _objectsByStartPosition.Add(elementLocation.Start, element);

                foreach (XAttribute attribute in element.Attributes())
                {
                    AttributeLocation attributeLocation = attribute.Annotation<AttributeLocation>();
                    if (attributeLocation == null)
                        continue;

                    _objectsByStartPosition.Add(attributeLocation.Range.Start, attribute);
                }
            }
            _objectRanges.AddRange(
                _objectsByStartPosition.Values.Select(
                    node => node.Annotation<NodeLocation>().Range
                )
            );
        }

        /// <summary>
        ///     Find the XML object (if any) at the specified position.
        /// </summary>
        /// <param name="position">
        ///     The target position .
        /// </param>
        /// <returns>
        ///     The XML object, or <c>null</c> if no object was found at the specified position.
        /// </returns>
        public XObject Find(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            // Internally, we always use 1-based indexing because this is what the System.Xml APIs (and I'd rather keep things simple).
            position = position.ToOneBased();
            
            // TODO: Consider if using binary search here would be worth the effort.

            Range lastMatchingRange = null;
            foreach (Range objectRange in _objectRanges)
            {
                if (position < objectRange)
                    continue;

                if (lastMatchingRange != null && objectRange > lastMatchingRange)
                    break; // No match.

                if (objectRange.Contains(position))
                    lastMatchingRange = objectRange;
            }   
            if (lastMatchingRange == null)
                return null;

            return _objectsByStartPosition[lastMatchingRange.Start];
        }
    }
}
