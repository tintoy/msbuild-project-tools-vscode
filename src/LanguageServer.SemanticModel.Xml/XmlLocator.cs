using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using Utilities;

    /// <summary>
    ///     A facility for looking up XML by textual location.
    /// </summary>
    public class XmlLocator
    {
        /// <summary>
        ///     The ranges for all XML nodes in the document
        /// </summary>
        /// <remarks>
        ///     Sorted by range comparison (effectively, this means document order).
        /// </remarks>
        readonly List<Range> _nodeRanges = new List<Range>();

        /// <summary>
        ///     All nodes XML, keyed by starting position.
        /// </summary>
        /// <remarks>
        ///     Sorted by position comparison.
        /// </remarks>
        readonly SortedDictionary<Position, XSNode> _nodesByStartPosition = new SortedDictionary<Position, XSNode>();

        /// <summary>
        ///     The position-lookup for the underlying XML document text.
        /// </summary>
        readonly TextPositions _documentPositions;

        /// <summary>
        ///     Create a new <see cref="XmlLocator"/>.
        /// </summary>
        /// <param name="document">
        ///     The underlying XML document.
        /// </param>
        /// <param name="documentPositions">
        ///     The position-lookup for the underlying XML document text.
        /// </param>
        public XmlLocator(XmlDocumentSyntax document, TextPositions documentPositions)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            
            if (documentPositions == null)
                throw new ArgumentNullException(nameof(documentPositions));
            
            _documentPositions = documentPositions;

            List<XSNode> allNodes = document.GetSemanticModel(_documentPositions);
            foreach (XSNode node in allNodes)
            {
                _nodeRanges.Add(node.Range);
                _nodesByStartPosition.Add(node.Range.Start, node);
            }

            SortNodeRanges();
        }

        /// <summary>
        ///     All nodes in the document.
        /// </summary>
        public IEnumerable<XSNode> AllNodes => _nodesByStartPosition.Values;

        /// <summary>
        ///     Inspect the specified position in the XML.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     An <see cref="XmlPosition"/> representing the result of the inspection.
        /// </returns>
        public XmlPosition Inspect(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            // Internally, we always use 1-based indexing because this is what the System.Xml APIs (and I'd rather keep things simple).
            position = position.ToOneBased();

            XSNode nodeAtPosition = FindNode(position);
            if (nodeAtPosition == null)
                return null;

            // If we're on the (seamless) boundary between 2 nodes, select the next node.
            if (nodeAtPosition.NextSibling != null)
            {
                if (position == nodeAtPosition.Range.End && position == nodeAtPosition.NextSibling.Range.Start)
                {
                    Serilog.Log.Logger.Debug("XmlLocator.Inspect moves to next sibling ({NodeKind} @ {NodeRange} -> {NextSiblingKind} @ {NextSiblingRange}).",
                        nodeAtPosition.Kind,
                        nodeAtPosition.Range,
                        nodeAtPosition.NextSibling.Kind,
                        nodeAtPosition.NextSibling.Range
                    );

                    nodeAtPosition = nodeAtPosition.NextSibling;
                }
            }

            int absolutePosition = _documentPositions.GetAbsolutePosition(position);

            XmlPosition inspectionResult = new XmlPosition(position, absolutePosition, nodeAtPosition);

            return inspectionResult;
        }

        /// <summary>
        ///     Inspect the specified position in the XML.
        /// </summary>
        /// <param name="absolutePosition">
        ///     The target position (0-based).
        /// </param>
        /// <returns>
        ///     An <see cref="XmlPosition"/> representing the result of the inspection.
        /// </returns>
        public XmlPosition Inspect(int absolutePosition)
        {
            if (absolutePosition < 0)
                throw new ArgumentOutOfRangeException(nameof(absolutePosition), absolutePosition, "Absolute position cannot be less than 0.");

            return Inspect(
                _documentPositions.GetPosition(absolutePosition)
            );
        }

        /// <summary>
        ///     Find the node (if any) at the specified position.
        /// </summary>
        /// <param name="position">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     The node, or <c>null</c> if no node was found at the specified position.
        /// </returns>
        public XSNode FindNode(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            // Internally, we always use 1-based indexing because this is what the MSBuild APIs use (and I'd rather keep things simple).
            position = position.ToOneBased();

            // Short-circuit.
            if (_nodesByStartPosition.TryGetValue(position, out XSNode exactMatch))
                return exactMatch;

            // TODO: Use binary search.

            Range lastMatchingRange = null;
            foreach (Range objectRange in _nodeRanges)
            {
                if (lastMatchingRange != null && objectRange.End > lastMatchingRange.End)
                    break; // We've moved past the end of the last matching range.

                if (objectRange.Contains(position))
                    lastMatchingRange = objectRange;
            }   
            if (lastMatchingRange == null)
                return null;

            return _nodesByStartPosition[lastMatchingRange.Start];
        }

        /// <summary>
        ///     Ensure that the locator's object ranges are sorted by start position, then end position.
        /// </summary>
        void SortNodeRanges()
        {
            Range[] unsortedRanges = _nodeRanges.ToArray();
            _nodeRanges.Clear();
            _nodeRanges.AddRange(
                unsortedRanges
                    .OrderBy(range => range.Start)
                    .ThenBy(range => range.End)
            );
        }
    }
}
