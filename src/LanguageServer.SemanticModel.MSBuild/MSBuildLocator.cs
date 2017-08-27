using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using Utilities;

    /// <summary>
    ///     A facility for looking up MSBuild project members by textual location.
    /// </summary>
    public class MSBuildLocator
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
        ///     The full path to the MSBuild project file.
        /// </summary>
        readonly string _projectFile;

        /// <summary>
        ///     The project XML.
        /// </summary>
        readonly XmlDocumentSyntax _projectXml;

        /// <summary>
        ///     The position-lookup for the project XML.
        /// </summary>
        readonly TextPositions _xmlPositions;

        /// <summary>
        ///     Create a new <see cref="MSBuildLocator"/>.
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
        public MSBuildLocator(Project project, XmlDocumentSyntax projectXml, TextPositions xmlPositions)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (projectXml == null)
                throw new ArgumentNullException(nameof(projectXml));
            
            if (xmlPositions == null)
                throw new ArgumentNullException(nameof(xmlPositions));
            
            _project = project;
            _projectFile = _project.FullPath ?? String.Empty;
            _projectXml = projectXml;
            _xmlPositions = xmlPositions;
            
            AddTargets();
            AddProperties();
            AddItems();
            AddImports();

            SortObjectRanges();
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

            // Internally, we always use 1-based indexing because this is what the System.Xml APIs use (and I'd rather keep things simple).
            position = position.ToOneBased();

            // Short-circuit.
            if (_objectsByStartPosition.TryGetValue(position, out MSBuildObject exactMatch))
                return exactMatch;

            // TODO: Use binary search.

            Range lastMatchingRange = null;
            foreach (Range objectRange in _objectRanges)
            {
                if (lastMatchingRange != null && objectRange.End > lastMatchingRange.End)
                    break; // We've moved past the end of the last matching range.

                if (objectRange.Contains(position))
                    lastMatchingRange = objectRange;
            }
            if (lastMatchingRange == null)
                return null;

            return _objectsByStartPosition[lastMatchingRange.Start];
        }

        /// <summary>
        ///     Determine whether an element is located in the current project.
        /// </summary>
        /// <param name="element">
        ///     The project element.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the element is from the current project; otherwise, <c>false</c>.
        /// </returns>
        bool IsFromCurrentProject(ProjectElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element.Location == null)
                return false;
            
            return String.Equals(element.Location.File, _projectFile, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Find the XML (if any) at the specified position.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     A <see cref="SyntaxNode"/> representing the element, or <c>null</c> if no element was found at the specified position.
        /// </returns>
        SyntaxNode FindXmlAtPosition(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            return _projectXml.FindNode(position, _xmlPositions);   
        }

        /// <summary>
        ///     Get a <see cref="Range"/> representing the span of XML within the source text.
        /// </summary>
        /// <param name="xml">
        ///     A <see cref="SyntaxNode"/> representing the XML.
        /// </param>
        /// <returns>
        ///     The <see cref="Range"/>.
        /// </returns>
        Range GetRange(SyntaxNode xml)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));
            
            return xml.Span.ToNative(_xmlPositions);
        }

        /// <summary>
        ///     Ensure that the locator's object ranges are sorted by start position, then end position.
        /// </summary>
        void SortObjectRanges()
        {
            _objectRanges.Sort();
        }

        /// <summary>
        ///     Add all targets defined in the project.
        /// </summary>
        void AddTargets()
        {
            foreach (ProjectTargetElement target in _project.Xml.Targets)
            {
                if (IsFromCurrentProject(target))
                    AddTarget(target);
            }
        }

        /// <summary>
        ///     Add a target.
        /// </summary>
        /// <param name="target">
        ///     The target's declaring <see cref="ProjectTargetElement"/>.
        /// </param>
        void AddTarget(ProjectTargetElement target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            
            Position propertyStart = target.Location.ToNative();
            
            SyntaxNode xmlAtPosition = FindXmlAtPosition(propertyStart);
            if (xmlAtPosition == null)
                return;

            XmlElementSyntaxBase targetElement = xmlAtPosition.GetContainingElement();
            if (targetElement == null)
                return;

            Range targetRange = GetRange(targetElement);

            _objectRanges.Add(targetRange);
            _objectsByStartPosition.Add(targetRange.Start,
                new MSBuildTarget(target, targetElement, targetRange)
            );
        }

        /// <summary>
        ///     Add all properties defined in the project.
        /// </summary>
        void AddProperties()
        {
            foreach (ProjectPropertyElement property in _project.Xml.Properties)
            {
                if (IsFromCurrentProject(property))
                    AddProperty(property);
            }
        }

        /// <summary>
        ///     Add a property.
        /// </summary>
        /// <param name="property">
        ///     The property's declaring <see cref="ProjectPropertyElement"/>.
        /// </param>
        void AddProperty(ProjectPropertyElement property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            
            Position propertyStart = property.Location.ToNative();
                
            SyntaxNode xmlAtPosition = FindXmlAtPosition(propertyStart);
            if (xmlAtPosition == null)
                return;

            XmlElementSyntaxBase propertyElement = xmlAtPosition.GetContainingElement();
            if (propertyElement == null)
                return;

            Range propertyRange = GetRange(propertyElement);
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

        /// <summary>
        ///     Add all items defined in the project.
        /// </summary>
        void AddItems()
        {
            // First, map each item to the element in the XML from where it originates.
            var itemsByXml = new Dictionary<ProjectItemElement, List<ProjectItem>>();
            foreach (ProjectItem item in _project.ItemsIgnoringCondition)
            {
                if (!IsFromCurrentProject(item.Xml))
                    continue; // Not declared in main project file.

                List<ProjectItem> itemsFromXml;
                if (!itemsByXml.TryGetValue(item.Xml, out itemsFromXml))
                {
                    itemsFromXml = new List<ProjectItem>();
                    itemsByXml.Add(item.Xml, itemsFromXml);
                }

                itemsFromXml.Add(item);
            }

            // Now process item elements and their associated items.
            HashSet<ProjectItem> usedItems = new HashSet<ProjectItem>(_project.Items);
            foreach (ProjectItemElement itemXml in itemsByXml.Keys)
            {
                Position itemStart = itemXml.Location.ToNative();

                SyntaxNode xmlAtPosition = _projectXml.FindNode(itemStart, _xmlPositions);
                if (xmlAtPosition == null)
                    continue;

                XmlElementSyntaxBase itemElement = xmlAtPosition.GetContainingElement();
                if (itemElement == null)
                    continue;

                Range itemRange = GetRange(itemElement);

                List<ProjectItem> itemsFromXml;
                if (!itemsByXml.TryGetValue(itemXml, out itemsFromXml)) // AF: Should not happen.
                    throw new InvalidOperationException($"Found item XML at {itemRange} with no corresponding items in the MSBuild project (irrespective of condition).");

                _objectRanges.Add(itemRange);
                if (usedItems.Contains(itemsFromXml[0]))
                {
                    _objectsByStartPosition.Add(itemRange.Start,
                        new MSBuildItemGroup(itemsByXml[itemXml], itemXml, itemElement, itemRange)
                    );
                }
                else
                {
                    _objectsByStartPosition.Add(itemRange.Start,
                        new MSBuildUnusedItemGroup(itemsByXml[itemXml], itemXml, itemElement, itemRange)
                    );
                }
            }
        }

        /// <summary>
        ///     Add all imports defined in the project.
        /// </summary>
        /// <remarks>
        ///     Currently, this doesn't capture imports whose condition evaluates to false.
        /// </remarks>
        void AddImports()
        {
            HashSet<ProjectImportElement> resolvedImportElements = new HashSet<ProjectImportElement>();
            var importsBySdk =
                _project.Imports.Where(import =>
                    IsFromCurrentProject(import.ImportingElement)
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
                    AddSdkImport(importGroup);
                else
                    AddImport(importGroup);

                resolvedImportElements.UnionWith(importGroup.Select(
                    resolvedImport => resolvedImport.ImportingElement
                ));
            }

            HashSet<ProjectImportElement> unresolvedImportElements = new HashSet<ProjectImportElement>(_project.Xml.Imports);
            unresolvedImportElements.ExceptWith(resolvedImportElements);

            foreach (ProjectImportElement importElement in unresolvedImportElements)
            {
                if (!String.IsNullOrWhiteSpace(importElement.Sdk))
                    AddUnresolvedSdkImport(importElement);
                else
                    AddUnresolvedImport(importElement);
            }
        }

        /// <summary>
        ///     Add an SDK-style import.
        /// </summary>
        /// <param name="resolvedImports">
        ///     The resolved imports resulting from the import declaration.
        /// </param>
        void AddSdkImport(IEnumerable<ResolvedImport> resolvedImports)
        {
            if (resolvedImports == null)
                throw new ArgumentNullException(nameof(resolvedImports));
            
            ResolvedImport firstImport = resolvedImports.First();
            Position importStart = firstImport.ImportingElement.Location.ToNative();

            // If the Sdk attribute is on the Project element rather than an import element, then the location reported by MSBuild will be invalid (go figure).
            if (importStart == Position.Invalid)
                importStart = Position.Origin;
            
            SyntaxNode xmlAtPosition = FindXmlAtPosition(importStart);
            if (xmlAtPosition == null)
                return;

            XmlElementSyntaxBase importElement = xmlAtPosition.GetContainingElement();
            if (importElement == null)
                return;

            XmlAttributeSyntax sdkAttribute = importElement.AsSyntaxElement["Sdk"];
            if (sdkAttribute == null)
                return;

            Range importRange = GetRange(sdkAttribute);
            _objectRanges.Add(importRange);
            _objectsByStartPosition.Add(importRange.Start,
                new MSBuildSdkImport(resolvedImports.ToArray(), sdkAttribute, importRange)
            );
        }

        /// <summary>
        ///     Add a regular-style import.
        /// </summary>
        /// <param name="resolvedImports">
        ///     The resolved imports resulting from the import declaration.
        /// </param>
        void AddImport(IEnumerable<ResolvedImport> resolvedImports)
        {
            if (resolvedImports == null)
                throw new ArgumentNullException(nameof(resolvedImports));

            var importsByImportingElement = resolvedImports.GroupBy(import => import.ImportingElement);
            foreach (var importsForImportingElement in importsByImportingElement)
            {
                Position importStart = importsForImportingElement.Key.Location.ToNative();
                
                SyntaxNode xmlAtPosition = FindXmlAtPosition(importStart);
                if (xmlAtPosition == null)
                    continue;

                XmlElementSyntaxBase importElement = xmlAtPosition.GetContainingElement();
                if (importElement == null)
                    continue;

                Range importRange = GetRange(importElement);
                _objectRanges.Add(importRange);
                _objectsByStartPosition.Add(importRange.Start,
                    new MSBuildImport(importsForImportingElement.ToArray(), importElement, importRange)
                );
            }
        }

        /// <summary>
        ///     Add an unresolved SDK-style import (i.e. condition is false).
        /// </summary>
        /// <param name="import">
        ///     The declaring import element.
        /// </param>
        void AddUnresolvedSdkImport(ProjectImportElement import)
        {
            if (import == null)
                throw new ArgumentNullException(nameof(import));
            
            Position importStart = import.Location.ToNative();

            SyntaxNode xmlAtPosition = FindXmlAtPosition(importStart);
            if (xmlAtPosition == null)
                return;

            XmlElementSyntaxBase importElement = xmlAtPosition.GetContainingElement();
            if (importElement == null)
                return;

            XmlAttributeSyntax sdkAttribute = importElement.AsSyntaxElement["Sdk"];

            Range importRange = GetRange(importElement);
            _objectRanges.Add(importRange);
            _objectsByStartPosition.Add(importRange.Start,
                new MSBuildUnresolvedSdkImport(import, sdkAttribute, importRange)
            );
        }

        /// <summary>
        ///     Add an unresolved regular-style import (i.e. condition is false).
        /// </summary>
        /// <param name="import">
        ///     The declaring import element.
        /// </param>
        void AddUnresolvedImport(ProjectImportElement import)
        {
            if (import == null)
                throw new ArgumentNullException(nameof(import));

            Position importStart = import.Location.ToNative();

            SyntaxNode xmlAtPosition = FindXmlAtPosition(importStart);
            if (xmlAtPosition == null)
                return;

            XmlElementSyntaxBase importElement = xmlAtPosition.GetContainingElement();
            if (importElement == null)
                return;

            Range importRange = GetRange(importElement);
            _objectRanges.Add(importRange);
            _objectsByStartPosition.Add(importRange.Start,
                new MSBuildUnresolvedImport(import, importElement, importRange)
            );            
        }
    }
}
