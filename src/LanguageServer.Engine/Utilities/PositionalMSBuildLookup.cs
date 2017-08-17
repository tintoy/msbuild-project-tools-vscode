using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    using XmlParser;

    public class PositionalMSBuildLookup
    {
        /// <summary>
        ///     The ranges for all XML objects in the document with positional annotations.
        /// </summary>
        /// <remarks>
        ///     Sorted by range comparison (effectively, this means document order).
        /// </remarks>
        readonly List<Range> _objectRanges = new List<Range>();

        readonly Project _project;

        /// <summary>
        ///     All objects in the project, keyed by starting position.
        /// </summary>
        /// <remarks>
        ///     Sorted by range comparison.
        /// </remarks>
        readonly SortedDictionary<Position, object> _objectsByStartPosition = new SortedDictionary<Position, object>();

        public PositionalMSBuildLookup(Project project, XmlDocumentSyntax projectXml, TextPositions xmlPositions)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            
            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));
            
            _project = project;

            // string projectFilePath = _project.FullPath ?? String.Empty;
            // foreach (ProjectProperty property in _project.Properties)
            // {
            //     if (property.Xml == null || property.Xml.Location.File != projectFilePath)
            //         continue; // Not declared in main project file.

            //     Position propertyStart = new Position(
            //         property.Xml.Location.Line,
            //         property.Xml.Location.Column
            //     );
                
            //     IXmlElementSyntax propertyElement = SyntaxLocator.FindNode(projectXml,
            //         position: xmlPositions.GetAbsolutePosition(propertyStart)
            //     ) as IXmlElementSyntax;
            //     if (propertyElement == null)
            //         continue;

            //     Range propertyRange = new Range(
            //         start: new Position()
            //     );

            //     _objectRanges.Add(propertyRange);
            //     _objectsByStartPosition.Add(propertyRange.Start, property);
            // }

            // foreach (ProjectItem item in _project.Items)
            // {
            //     if (item.Xml == null || item.Xml.Location.File != projectFilePath)
            //         continue; // Not declared in main project file.

            //     Position itemStart = new Position(
            //         item.Xml.Location.Line,
            //         item.Xml.Location.Column
            //     );
                
            //     XElement itemElement = xmlPositions.Find(itemStart) as XElement;
            //     if (itemElement == null)
            //         continue;

            //     Range itemRange = itemElement.Annotation<NodeLocation>().Range;

            //     _objectRanges.Add(itemRange);
            //     _objectsByStartPosition.Add(itemRange.Start, item);
            // }

            // _objectRanges.Sort();
        }

        /// <summary>
        ///     Find the project object (if any) at the specified position.
        /// </summary>
        /// <param name="position">
        ///     The target position .
        /// </param>
        /// <returns>
        ///     The project object, or <c>null</c> if no object was found at the specified position.
        /// </returns>
        public object Find(Position position)
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
