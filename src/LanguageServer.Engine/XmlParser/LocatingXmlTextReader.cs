using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.XmlParser
{
    /// <summary>
    ///     An <see cref="XmlTextReader"/> that captures location information for elements and attributes while reading XML.
    /// </summary>
    /// <remarks>
    ///     This reader is only intended to be used via <see cref="XDocument.Load(XmlReader)"/>; it is sensitive to the order in which its methods are called.
    /// 
    ///     TODO: Consider exposing an <see cref="IObservable{T}"/> of location information (this would make it easier to isolate the logic for capturing node start / end locations).
    /// </remarks>
    public class LocatingXmlTextReader
        : XmlTextReader
    {
        /// <summary>
        ///     Locations captured by the reader.
        /// </summary>
        readonly List<NodeLocation> _locations = new List<NodeLocation>();

        /// <summary>
        ///     A stack containing the elements being processed.
        /// </summary>        
        readonly Stack<ElementLocation> _elementLocationStack = new Stack<ElementLocation>();

        /// <summary>
        ///     Should we treat the current position as the end of the most-recent element?
        /// </summary>
        bool _emitEndElement;

        /// <summary>
        ///     Create a new <see cref="LocatingXmlTextReader"/>.
        /// </summary>
        /// <param name="input">
        ///     The <see cref="TextReader"/> to read from.
        /// </param>
        internal LocatingXmlTextReader(TextReader input)
            : base(input)
        {
        }

        /// <summary>
        ///     Locations captured by the reader.
        /// </summary>
        public IReadOnlyList<NodeLocation> Locations => _locations;

        /// <summary>
        ///     A <see cref="Position"/> representing the current element position.
        /// </summary>
        Position CurrentPosition => new Position(LineNumber, LinePosition);

        /// <summary>
        ///     Read the next node.
        /// </summary>
        /// <returns>
        ///     <c>true</c>, if a node was read; otherwise, <c>false</c> (i.e. end of XML).
        /// </returns>
        public override bool Read()
        {
            Log.Verbose("{NodeType} {Name} ({LineNumber},{ColumnNumber}) -> [Read]",
                NodeType, Name, LineNumber, LinePosition
            );

            bool result = base.Read();
            if (!result)
                return false;

            Log.Verbose("[Read] -> {NodeType} {Name} ({LineNumber},{ColumnNumber})",
                NodeType, Name, LineNumber, LinePosition
            );

            CaptureLocation();

            return true;
        }

        /// <summary>
        ///     Move to the first attribute of the current element.
        /// </summary>
        /// <returns>
        ///     <c>true</c>, if an attribute is available; otherwise, <c>false</c> (i.e. no attributes available).
        /// </returns>
        public override bool MoveToFirstAttribute()
        {
            Log.Verbose("{NodeType} {Name} ({LineNumber},{LinePosition}) -> [FirstAttribute]",
                NodeType, Name, LineNumber, LinePosition
            );

            bool result = base.MoveToFirstAttribute();
            if (!result)
                return false;

            Log.Verbose("-> [FirstAttribute] {NodeType} {Name} ({LineNumber},{LinePosition})",
                NodeType, Name, LineNumber, LinePosition
            );

            CaptureLocation();

            return true;
        }

        /// <summary>
        ///     Move to the next attribute of the current element.
        /// </summary>
        /// <returns>
        ///     <c>true</c>, if an attribute is available; otherwise, <c>false</c> (i.e. no more attributes available).
        /// </returns>
        public override bool MoveToNextAttribute()
        {
            Log.Verbose("{NodeType} {Name} ({LineNumber},{LinePosition}) -> [NextAttribute]",
                NodeType, Name, LineNumber, LinePosition
            );

            bool result = base.MoveToNextAttribute();
            if (!result)
                return false;

            Log.Verbose("-> [NextAttribute] {NodeType} {Name} ({LineNumber},{LinePosition})",
                NodeType, Name, LineNumber, LinePosition
            );

            CaptureLocation();

            return true;
        }

        /// <summary>
        ///     Move back to the current element (after reading attributes).
        /// </summary>
        /// <returns>
        ///     <c>true</c>, if the reader successfully moved back to the current element; otherwise, <c>false</c> (i.e. not currently on an element).
        /// </returns>
        public override bool MoveToElement()
        {
            Log.Verbose("{NodeType} {Name} ({LineNumber},{LinePosition}) -> [Element]",
                NodeType, Name, LineNumber, LinePosition
            );

            bool result = base.MoveToElement();
            if (!result)
                return false;

            // If this is an empty element, capture its end position (even though there won't be a corresponding EndElement).
            if (IsEmptyElement)
                _emitEndElement = true;

            Log.Verbose("-> [Element] {NodeType} {Name} ({LineNumber},{LinePosition}) (IsEmpty={IsEmptyElement}, EmitEndElement={EmitEndElement})",
                NodeType, Name, LineNumber, LinePosition, IsEmptyElement, _emitEndElement
            );

            return true;
        }

        /// <summary>
        ///     Capture location information for the current element.
        /// </summary>
        void CaptureLocation()
        {
            Log.Verbose("[Capture{NodeType}Location] {Name} ({LineNumber},{ColumnNumber}) (IsEmpty={IsEmptyElement}, EmitEndElement={EmitEndElement}, ElementLocationStack={StackDepth})",
                NodeType, Name, LineNumber, LinePosition, IsEmptyElement, _emitEndElement, DumpElementStack()
            );

            // This logic is pretty ugly, and needs serious reworking (would work better modeled as a state machine).
            // It's functional, there are almost certainly edge-cases it will fail (e.g. the last element).
            if (NodeType == XmlNodeType.Element)
            {
                ElementLocation elementLocation;
                if (_emitEndElement)
                {
                    // Element followed by element; capture the end of the previous element.
                    elementLocation = _elementLocationStack.Pop();
                    elementLocation.Range = elementLocation.Range.WithEnd(
                        CurrentPosition.Move(columnCount: -1)
                    );

                    Log.Verbose("[Capture{NodeType}LocationEnd] {Name} ({StartLineNumber},{StartColumnNumber}-{EndLineNumber},{EndColumnNumber})",
                        NodeType, Name,
                        elementLocation.Start.LineNumber, elementLocation.Start.ColumnNumber,
                        elementLocation.End.LineNumber, elementLocation.End.ColumnNumber
                    );

                    _emitEndElement = false;
                }

                Log.Verbose("[Capture{NodeType}LocationStart] {Name} ({LineNumber},{ColumnNumber})",
                    NodeType, Name, LineNumber, LinePosition
                );

                // Capture the start of the new element.
                elementLocation = new ElementLocation
                {
                    Depth = Depth,
                    Range = Range.FromPosition(
                        CurrentPosition.Move(columnCount: -1)
                    )
                };
                _locations.Add(elementLocation);
                _elementLocationStack.Push(elementLocation);
            }
            else if (NodeType == XmlNodeType.EndElement || _emitEndElement)
            {
                // Element, followed by whitespace / text, followed by element.
                ElementLocation elementLocation = _elementLocationStack.Pop();
                Position endPosition = CurrentPosition;
                if (NodeType == XmlNodeType.EndElement)
                    endPosition = endPosition.Move(columnCount: Name.Length + 1 /* > */);

                elementLocation.Range = elementLocation.Range.WithEnd(endPosition);

                Log.Verbose("[Capture{NodeType}LocationEnd] {Name} ({StartLineNumber},{StartColumnNumber}-{EndLineNumber},{EndColumnNumber})",
                    NodeType, Name,
                    elementLocation.Start.LineNumber, elementLocation.Start.ColumnNumber,
                    elementLocation.End.LineNumber, elementLocation.End.ColumnNumber
                );

                _emitEndElement = false;
            }
            else if (NodeType == XmlNodeType.Attribute)
            {
                // Attribute
                _locations.Add(
                    AttributeLocation.Create(CurrentPosition, Name, Value)
                );
            }
            else
            {
                // We don't care, but log it anyway to help track down weird edge-case behaviour.
                Log.Verbose("[SkipCapture{NodeType}Location] {Name} ({LineNumber},{ColumnNumber})",
                    NodeType, Name, LineNumber, LinePosition
                );
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
        public static XDocument LoadWithLocations(string filePath)
        {
            using (StreamReader reader = File.OpenText(filePath))
            {
                return LoadWithLocations(reader);
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
        public static XDocument LoadWithLocations(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return LoadWithLocations(reader);
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
        public static XDocument LoadWithLocations(TextReader textReader)
        {
            XDocument document;
            using (LocatingXmlTextReader xmlReader = new LocatingXmlTextReader(textReader))
            {
                document = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
                AddLocations(document.Root,
                    new Queue<NodeLocation>(xmlReader._locations)
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

        /// <summary>
        ///     Dump the current element stack.
        /// </summary>
        /// <returns>
        ///     A string representation of the element stack.
        /// </returns>
        /// <remarks>
        ///     For diagnostic purposes.
        /// </remarks>
        string DumpElementStack()
        {
            return "[" + String.Join(", ", _elementLocationStack.Reverse().Select(location => location.Name)) + "]";
        }
    }
}
