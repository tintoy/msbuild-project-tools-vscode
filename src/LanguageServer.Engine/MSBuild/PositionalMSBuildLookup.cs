using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    using System.Linq;
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
                if (!String.Equals(target.Location.File, projectFilePath, StringComparison.OrdinalIgnoreCase))
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

            foreach (ProjectPropertyElement property in _project.Xml.Properties)
            {
                Position propertyStart = property.Location.ToNative();
                
                SyntaxNode xmlAtPosition = projectXml.FindNode(propertyStart, xmlPositions);
                if (xmlAtPosition == null)
                    continue;

                XmlElementSyntaxBase propertyElement = xmlAtPosition.GetContainingElement();
                if (propertyElement == null)
                    continue;

                Range propertyRange = propertyElement.Span.ToNative(xmlPositions);
                _objectRanges.Add(propertyRange);

                ProjectProperty evaluatedProperty = _project.GetProperty(property.Name);
                if (evaluatedProperty != null)
                {
                    _objectsByStartPosition.Add(propertyRange.Start,
                        new MSBuildProperty(evaluatedProperty, property, propertyElement, propertyRange)
                    );
                }
                else
                {
                    _objectsByStartPosition.Add(propertyRange.Start,
                        new MSBuildUnusedProperty(property, propertyElement, propertyRange)
                    );
                }
            }

            HashSet<ProjectItem> usedItems = new HashSet<ProjectItem>(_project.Items);

            var itemsByXml = new Dictionary<ProjectItemElement, List<ProjectItem>>();
            foreach (ProjectItem item in _project.ItemsIgnoringCondition)
            {
                if (item.Xml == null || !String.Equals(item.Xml.Location.File, projectFilePath, StringComparison.OrdinalIgnoreCase))
                    continue; // Not declared in main project file.

                List<ProjectItem> itemsFromXml;
                if (!itemsByXml.TryGetValue(item.Xml, out itemsFromXml))
                {
                    itemsFromXml = new List<ProjectItem>();
                    itemsByXml.Add(item.Xml, itemsFromXml);
                }

                itemsFromXml.Add(item);
            }
            foreach (ProjectItemElement itemXml in itemsByXml.Keys)
            {
                Position itemStart = itemXml.Location.ToNative();

                SyntaxNode xmlAtPosition = projectXml.FindNode(itemStart, xmlPositions);
                if (xmlAtPosition == null)
                    continue;

                XmlElementSyntaxBase itemElement = xmlAtPosition.GetContainingElement();
                if (itemElement == null)
                    continue;

                Range itemRange = itemElement.Span.ToNative(xmlPositions);

                List<ProjectItem> itemsFromXml;
                if (!itemsByXml.TryGetValue(itemXml, out itemsFromXml)) // AF: Should not happen.
                    throw new InvalidOperationException($"Found item XML at {itemRange} with no corresponding items in the MSBuild project (irrespective of condition).");

                _objectRanges.Add(itemRange);
                if (usedItems.Contains(itemsFromXml[0]))
                {
                    Serilog.Log.Information("{Name} item group spanning {Range}", itemsFromXml[0].ItemType, itemRange);

                    _objectsByStartPosition.Add(itemRange.Start,
                        new MSBuildItemGroup(itemsByXml[itemXml], itemXml, itemElement, itemRange)
                    );
                }
                else
                {
                    Serilog.Log.Information("Unused {Name} item group spanning {Range}", itemsFromXml[0].ItemType, itemRange);

                    _objectsByStartPosition.Add(itemRange.Start,
                        new MSBuildUnusedItemGroup(itemsByXml[itemXml], itemXml, itemElement, itemRange)
                    );
                }
            }

            var importsBySdk =
                project.Imports.Where(import =>
                    String.Equals(import.ImportingElement.Location.File, projectFilePath, StringComparison.OrdinalIgnoreCase)
                    &&
                    import.ImportedProject.Location != null
                )
                .GroupBy(
                    import => import.ImportingElement.Sdk
                );
            foreach (var importGroup in importsBySdk)
            {
                string importingSdk = importGroup.Key;
                if (importingSdk != String.Empty)
                {
                    // An SDK-style import.
                    ResolvedImport firstImport = importGroup.First();
                    Position importStart = firstImport.ImportingElement.Location.ToNative();

                    // If the Sdk attribute is on the Project element rather than an import element, then the location reported by MSBuild will be invalid (go figure).
                    if (importStart == Position.Invalid)
                        importStart = Position.Origin;
                    
                    SyntaxNode xmlAtPosition = projectXml.FindNode(importStart, xmlPositions);
                    if (xmlAtPosition == null)
                        continue;

                    XmlElementSyntaxBase importElement = xmlAtPosition.GetContainingElement();
                    if (importElement == null)
                        continue;

                    XmlAttributeSyntax sdkAttribute = importElement.AsSyntaxElement["Sdk"];
                    Range importRange = sdkAttribute.Span.ToNative(xmlPositions);

                    _objectRanges.Add(importRange);
                    _objectsByStartPosition.Add(importRange.Start,
                        new MSBuildSdkImport(importGroup.ToArray(), sdkAttribute, importRange)
                    );
                }
                else
                {
                    // A regular import (each element may result in multiple imports).
                    var importsByImportingElement = importGroup.GroupBy(import => import.ImportingElement);
                    foreach (var importsForImportingElement in importsByImportingElement)
                    {
                        Position importStart = importsForImportingElement.Key.Location.ToNative();
                        
                        SyntaxNode xmlAtPosition = projectXml.FindNode(importStart, xmlPositions);
                        if (xmlAtPosition == null)
                            continue;

                        XmlElementSyntaxBase importElement = xmlAtPosition.GetContainingElement();
                        if (importElement == null)
                            continue;

                        Range importRange = importElement.Span.ToNative(xmlPositions);
                        _objectRanges.Add(importRange);
                        _objectsByStartPosition.Add(importRange.Start,
                            new MSBuildImport(importsForImportingElement.ToArray(), importElement, importRange)
                        );
                    }
                    
                }
            }

            _objectRanges.Sort();
        }

        /// <summary>
        ///     All known MSBuild objects.
        /// </summary>
        public IEnumerable<MSBuildObject> AllObjects => _objectsByStartPosition.Values;

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
