using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    using Utilities;

    /// <summary>
    ///     A facility for looking up MSBuild project members by textual location.
    /// </summary>
    public class PositionalMSBuildLookup
    {
        /// <summary>
        ///     The ranges for all XML objects in the document with positional annotations.
        /// </summary>
        /// <remarks>
        ///     Sorted by range comparison (effectively, this means document order).
        /// </remarks>
        readonly List<Range> _objectRanges = new List<Range>();

        /// <summary>
        ///     All objects in the project, keyed by starting position.
        /// </summary>
        /// <remarks>
        ///     Sorted by range comparison.
        /// </remarks>
        readonly SortedDictionary<Position, MSBuildObject> _objectsByStartPosition = new SortedDictionary<Position, MSBuildObject>();

        /// <summary>
        ///     The MSBuild project.
        /// </summary>
        readonly Project _project;

        /// <summary>
        ///     Create a new <see cref="PositionalMSBuildLookup"/>.
        /// </summary>
        /// <param name="project">
        ///     The MSBuild project.
        /// </param>
        /// <param name="projectXml">
        ///     The project XML.
        /// </param>
        /// <param name="xmlPositions">
        ///     The position-lookup for the project XML.
        /// </param>
        public PositionalMSBuildLookup(Project project, XmlDocumentSyntax projectXml, TextPositions xmlPositions)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            
            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));
            
            _project = project;

            string projectFilePath = _project.FullPath ?? String.Empty;
            foreach (ProjectTargetElement target in _project.Xml.Targets)
            {
                if (target.Location.File != projectFilePath)
                    continue; // Not declared in main project file.

                Position propertyStart = target.Location.ToNative();
                
                SyntaxNode xmlAtPosition = projectXml.FindNode(propertyStart, xmlPositions);
                if (xmlAtPosition == null)
                    continue;

                XmlElementSyntaxBase targetElement = xmlAtPosition.GetContainingElement();
                if (targetElement == null)
                    continue;

                Range targetRange = targetElement.Span.ToNative(xmlPositions);

                _objectRanges.Add(targetRange);
                _objectsByStartPosition.Add(targetRange.Start,
                    new MSBuildTarget(target, targetElement, targetRange)
                );
            }

            foreach (ProjectProperty property in _project.Properties)
            {
                if (property.Xml == null || property.Xml.Location.File != projectFilePath)
                    continue; // Not declared in main project file.

                Position propertyStart = property.Xml.Location.ToNative();
                
                SyntaxNode xmlAtPosition = projectXml.FindNode(propertyStart, xmlPositions);
                if (xmlAtPosition == null)
                    continue;

                XmlElementSyntaxBase propertyElement = xmlAtPosition.GetContainingElement();
                if (propertyElement == null)
                    continue;

                Range propertyRange = propertyElement.Span.ToNative(xmlPositions);

                _objectRanges.Add(propertyRange);
                _objectsByStartPosition.Add(propertyRange.Start,
                    new MSBuildProperty(property, propertyElement, propertyRange)
                );
            }

            foreach (ProjectItem item in _project.Items)
            {
                if (item.Xml == null || item.Xml.Location.File != projectFilePath)
                    continue; // Not declared in main project file.

                Position itemStart = item.Xml.Location.ToNative();

                // HACK: One project item element can give rise to multiple project items; we don't support this yet and will therefore store only the first one.
                if (_objectsByStartPosition.ContainsKey(itemStart))
                    continue; // TODO: We should really store a list of all items in the group, instead of just the first item.
                
                SyntaxNode xmlAtPosition = projectXml.FindNode(itemStart, xmlPositions);
                if (xmlAtPosition == null)
                    continue;

                XmlElementSyntaxBase itemElement = xmlAtPosition.GetContainingElement();
                if (itemElement == null)
                    continue;

                Range itemRange = itemElement.Span.ToNative(xmlPositions);

                _objectRanges.Add(itemRange);
                _objectsByStartPosition.Add(itemRange.Start,
                    new MSBuildItem(item, itemElement, itemRange)
                );
            }

            _objectRanges.Sort();
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
        public MSBuildObject Find(Position position)
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
