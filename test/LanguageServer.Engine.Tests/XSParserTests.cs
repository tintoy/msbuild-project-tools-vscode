using Microsoft.Language.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using SemanticModel;
    using Utilities;

    /// <summary>
    ///     Tests for <see cref="XSParser"/>
    /// </summary>
    public sealed class XSParserTests
    {
        /// <summary>
        ///     The directory for test files.
        /// </summary>
        static readonly DirectoryInfo TestDirectory = new DirectoryInfo(Path.GetDirectoryName(
            new Uri(typeof(XSParserTests).Assembly.CodeBase).LocalPath
        ));

        /// <summary>
        ///     Create a new <see cref="XSParser"/> test suite.
        /// </summary>
        /// <param name="testOutput">
        ///     Output for the current test.
        /// </param>
        public XSParserTests(ITestOutputHelper testOutput)
        {
            if (testOutput == null)
                throw new ArgumentNullException(nameof(testOutput));

            TestOutput = testOutput;
        }

        /// <summary>
        ///     Output for the current test.
        /// </summary>
        ITestOutputHelper TestOutput { get; }

        /// <summary>
        ///     XSParser should discover the specified number of nodes.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="expectedNodeCount">
        ///     The expected number of nodes to be 
        /// </param>
        [InlineData("Test1", 12)]
        [Theory(DisplayName = "XSParser discovers node count ")]
        void NodeCount(string testFileName, int expectedNodeCount)
        {
            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions xmlPositions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            List<XSNode> nodes = xmlDocument.GetSemanticModel(xmlPositions);
            Assert.NotNull(nodes);
            Assert.Equal(expectedNodeCount, nodes.Count);
        }

        /// <summary>
        ///     XSParser should discover a node of the specified kind at the specified index.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="index">
        ///     The node's index within the semantic model.
        /// </param>
        /// <param name="nodeKind">
        ///     The node kind.
        /// </param>
        [InlineData("Test1", 0, XSNodeKind.Element)]
        [InlineData("Test1", 1, XSNodeKind.Whitespace)]
        [InlineData("Test1", 2, XSNodeKind.Element)]
        [InlineData("Test1", 3, XSNodeKind.Attribute)]
        [InlineData("Test1", 4, XSNodeKind.Whitespace)]
        [InlineData("Test1", 5, XSNodeKind.Element)]
        [InlineData("Test1", 6, XSNodeKind.Whitespace)]
        [InlineData("Test1", 7, XSNodeKind.Element)]
        [InlineData("Test1", 8, XSNodeKind.Whitespace)]
        [InlineData("Test1", 9, XSNodeKind.Whitespace)]
        [InlineData("Test1", 10, XSNodeKind.Element)]
        [InlineData("Test1", 11, XSNodeKind.Whitespace)]
        [InlineData("Invalid1.EmptyOpeningTag", 0, XSNodeKind.Element)]
        [InlineData("Invalid1.EmptyOpeningTag", 1, XSNodeKind.Whitespace)]
        [InlineData("Invalid1.EmptyOpeningTag", 2, XSNodeKind.Element)]
        [InlineData("Invalid1.EmptyOpeningTag", 3, XSNodeKind.Attribute)]
        [InlineData("Invalid1.EmptyOpeningTag", 4, XSNodeKind.Whitespace)]
        [InlineData("Invalid1.EmptyOpeningTag", 5, XSNodeKind.Element)]
        [InlineData("Invalid1.EmptyOpeningTag", 6, XSNodeKind.Whitespace)]
        [InlineData("Invalid1.EmptyOpeningTag", 7, XSNodeKind.Element)]
        [InlineData("Invalid1.EmptyOpeningTag", 8, XSNodeKind.Whitespace)]
        [InlineData("Invalid1.EmptyOpeningTag", 9, XSNodeKind.Element)] // Invalid element
        [InlineData("Invalid1.EmptyOpeningTag", 10, XSNodeKind.Whitespace)]
        [InlineData("Invalid1.EmptyOpeningTag", 11, XSNodeKind.Whitespace)]
        [InlineData("Invalid1.EmptyOpeningTag", 12, XSNodeKind.Element)]
        [Theory(DisplayName = "XSParser discovers node of kind ")]
        void NodeKind(string testFileName, int index, XSNodeKind nodeKind)
        {
            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions xmlPositions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            List<XSNode> nodes = xmlDocument.GetSemanticModel(xmlPositions);
            Assert.NotNull(nodes);
            Assert.InRange(index, 0, nodes.Count - 1);

            XSNode node = nodes[index];
            Assert.NotNull(node);

            Assert.Equal(nodeKind, node.Kind);
        }

        /// <summary>
        ///     XSParser should discover an invalid element at the specified index.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="index">
        ///     The element node's index within the semantic model.
        /// </param>
        [InlineData("Invalid1.EmptyOpeningTag", 9)]
        [InlineData("Invalid1.DoubleOpeningTag", 7)]
        [Theory(DisplayName = "XSParser discovers invalid element ")]
        void InvalidElement(string testFileName, int index)
        {
            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions xmlPositions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            List<XSNode> nodes = xmlDocument.GetSemanticModel(xmlPositions);
            Assert.NotNull(nodes);
            Assert.InRange(index, 0, nodes.Count - 1);

            XSNode node = nodes[index];
            Assert.NotNull(node);

            Assert.Equal(XSNodeKind.Element, node.Kind);
            Assert.False(node.IsValid, "IsValid");
        }

        /// <summary>
        ///     XSParser should discover a node with the specified range at the specified index.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="index">
        ///     The node index.
        /// </param>
        /// <param name="startLine">
        ///     The node's starting line.
        /// </param>
        /// <param name="startColumn">
        ///     The node's starting column.
        /// </param>
        /// <param name="endLine">
        ///     The node's ending line.
        /// </param>
        /// <param name="endColumn">
        ///     The node's ending column.
        /// </param>
        [InlineData("Test1", 0, 1, 1, 7, 12)]
        [InlineData("Test1", 1, 1, 11, 2, 5)]
        [InlineData("Test1", 2, 2, 5, 5, 16)]
        [InlineData("Test1", 3, 2, 15, 2, 34)]
        [InlineData("Test1", 4, 2, 36, 3, 9)]
        [InlineData("Test1", 5, 3, 9, 3, 21)]
        [InlineData("Test1", 6, 3, 21, 4, 9)]
        [InlineData("Test1", 7, 4, 9, 4, 21)]
        [InlineData("Test1", 8, 4, 21, 5, 5)]
        [InlineData("Test1", 9, 5, 16, 6, 5)]
        [InlineData("Test1", 10, 6, 5, 6, 26)]
        [InlineData("Test1", 11, 6, 26, 7, 1)]
        [Theory(DisplayName = "XSParser discovers node with range ")]
        void NodeRange(string testFileName, int index, int startLine, int startColumn, int endLine, int endColumn)
        {
            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions xmlPositions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            List<XSNode> nodes = xmlDocument.GetSemanticModel(xmlPositions);
            Assert.NotNull(nodes);
            Assert.InRange(index, 0, nodes.Count - 1);

            XSNode node = nodes[index];
            Assert.NotNull(node);

            TestOutput.WriteLine("Node {0} at {1} is {2}.",
                index,
                node.Range,
                node.Kind
            );

            Range expectedRange = new Range(
                start: new Position(startLine, startColumn),
                end: new Position(endLine, endColumn)
            );

            Assert.Equal(expectedRange, node.Range);
        }

        /// <summary>
        ///     XSParser should discover an element with the specified attributes range at the specified index.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="index">
        ///     The node index.
        /// </param>
        /// <param name="elementName">
        ///     The expected element name.
        /// </param>
        /// <param name="startLine">
        ///     The element's attributes node's starting line.
        /// </param>
        /// <param name="startColumn">
        ///     The element's attributes node's starting column.
        /// </param>
        /// <param name="endLine">
        ///     The element's attributes node's ending line.
        /// </param>
        /// <param name="endColumn">
        ///     The element's attributes node's ending column.
        /// </param>
        [InlineData("Test1", "Element2", 2, 15, 2, 35)]
        [InlineData("Test2", "PackageReference", 11, 27, 11, 70)]
        [Theory(DisplayName = "XSParser discovers element with attributes range ")]
        void ElementAttributesRange(string testFileName, string elementName, int startLine, int startColumn, int endLine, int endColumn)
        {
            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions xmlPositions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            List<XSNode> nodes = xmlDocument.GetSemanticModel(xmlPositions);
            Assert.NotNull(nodes);

            XSNode targetNode = nodes.Find(node => node.Name == elementName);
            Assert.NotNull(targetNode);

            Assert.IsAssignableFrom<XSElement>(targetNode);
            XSElement targetElement = (XSElement)targetNode;
            
            Range expectedRange = new Range(
                start: new Position(startLine, startColumn),
                end: new Position(endLine, endColumn)
            );

            Assert.Equal(expectedRange, targetElement.AttributesRange);
        }

        /// <summary>
        ///     Verify that the Parser correctly determines an invalid element's range.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="index">
        ///     The node index.
        /// </param>
        /// <param name="elementName">
        ///     The expected element name.
        /// </param>
        /// <param name="startLine">
        ///     The element's attributes node's starting line.
        /// </param>
        /// <param name="startColumn">
        ///     The element's attributes node's starting column.
        /// </param>
        /// <param name="endLine">
        ///     The element's attributes node's ending line.
        /// </param>
        /// <param name="endColumn">
        ///     The element's attributes node's ending column.
        /// </param>
        [Theory(DisplayName = "Invalid element has expected range ")]
        [InlineData("Invalid2.NoClosingTag", 22, "P", 10, 5, 10, 8)]
        public void InvalidElementRange(string testFileName, int nodeIndex, string elementName, int startLine, int startColumn, int endLine, int endColumn)
        {
            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions xmlPositions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            List<XSNode> nodes = xmlDocument.GetSemanticModel(xmlPositions);
            Assert.NotNull(nodes);

            XSNode targetNode = nodes[nodeIndex];
            Assert.NotNull(targetNode);

            Assert.IsAssignableFrom<XSElement>(targetNode);
            XSElement targetElement = (XSElement)targetNode;

            Assert.Equal(elementName, targetElement.Name);
            Assert.False(targetElement.IsValid, "IsValid");

            Range expectedRange = new Range(
                start: new Position(startLine, startColumn),
                end: new Position(endLine, endColumn)
            );
            Assert.Equal(expectedRange, targetElement.Range);
        }

        /// <summary>
        ///     Load a test file.
        /// </summary>
        /// <param name="relativePathSegments">
        ///     The file's relative path segments.
        /// </param>
        /// <returns>
        ///     The file content, as a string.
        /// </returns>
        static string LoadTestFile(params string[] relativePathSegments)
        {
            if (relativePathSegments == null)
                throw new ArgumentNullException(nameof(relativePathSegments));

            return File.ReadAllText(
                Path.Combine(
                    TestDirectory.FullName,
                    Path.Combine(relativePathSegments)
                )
            );
        }
    }
}
