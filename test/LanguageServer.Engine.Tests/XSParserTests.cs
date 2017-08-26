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
        ///     The node index.
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
        ///     XSParser should discover nodes of the specified type at the specified index.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="index">
        ///     The node index.
        /// </param>
        /// <param name="nodeKind">
        ///     The node kind.
        /// </param>
        [InlineData("Test1", 0, 1, 1, 7, 11)]
        [InlineData("Test1", 1, 1, 11, 2, 5)]
        [InlineData("Test1", 2, 2, 5, 5, 15)]
        [InlineData("Test1", 3, 2, 15, 2, 33)]
        [InlineData("Test1", 4, 2, 35, 3, 9)]
        [InlineData("Test1", 5, 3, 9, 3, 20)]
        [InlineData("Test1", 6, 3, 21, 4, 9)]
        [InlineData("Test1", 7, 4, 9, 4, 20)]
        [InlineData("Test1", 8, 4, 21, 5, 5)]
        [InlineData("Test1", 9, 5, 16, 6, 5)]
        [InlineData("Test1", 10, 6, 5, 6, 25)]
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

            Range expectedRange = new Range(
                start: new Position(startLine, startColumn),
                end: new Position(endLine, endColumn)
            );

            Assert.Equal(expectedRange, node.Range);
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
