using Microsoft.Language.Xml;
using System;
using System.IO;
using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using SemanticModel;
    using Utilities;

    /// <summary>
    ///     Tests for locating XML by position.
    /// </summary>
    public class XmlLocatorTests
    {
        /// <summary>
        ///     The directory for test files.
        /// </summary>
        static readonly DirectoryInfo TestDirectory = new DirectoryInfo(Path.GetDirectoryName(
            new Uri(typeof(XmlLocatorTests).Assembly.CodeBase).LocalPath
        ));

        /// <summary>
        ///     Verify that the target line and column lie on a <see cref="SyntaxList"/> inside element 1.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="line">
        ///     The target line.
        /// </param>
        /// <param name="column">
        ///     The target column.
        /// </param>
        [InlineData("Test1", 2, 5)]
        [InlineData("Test1", 3, 9)]
        [Theory(DisplayName = "Expect line and column to be inside element ")]
        public void Line_Col_ListInsideElement1(string testFileName, int line, int column)
        {
            Position testPosition = new Position(line, column);
            Console.WriteLine("Test Position: {0}", testPosition);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            int absolutePosition = positions.GetAbsolutePosition(testPosition) - 1; // To find out if we can insert an element, make sure we find the node at the position ONE BEFORE the insertion point!
            SyntaxNode foundNode = xmlDocument.FindNode(absolutePosition,
                descendIntoChildren: node => true
            );
            Assert.NotNull(foundNode);
            Assert.IsAssignableFrom<SyntaxList>(foundNode);
            SyntaxList list = (SyntaxList)foundNode;

            Range listSpan = list.Span.ToNative(positions);
            Assert.True(
                listSpan.Contains(testPosition),
                "List's span must contain the test position."
            );
        }

        /// <summary>
        ///     Verify that the target line and column lie after within an element's content.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="line">
        ///     The target line.
        /// </param>
        /// <param name="column">
        ///     The target column.
        /// </param>
        /// <param name="expectedNodeKind">
        ///     The kind of node expected at the position.
        /// </param>
        [InlineData("Test1", 3, 21, XSNodeKind.Whitespace)]
        [InlineData("Test1", 3, 22, XSNodeKind.Whitespace)]
        [InlineData("Test2", 11, 8, XSNodeKind.Whitespace)]
        [InlineData("Test2", 5, 22, XSNodeKind.Text)]
        [Theory(DisplayName = "Expect line and column to be within element content ")]
        public void Line_Col_InElementContent(string testFileName, int line, int column, XSNodeKind expectedNodeKind)
        {
            Position testPosition = new Position(line, column);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax document = Parser.ParseText(testXml);

            XmlLocator locator = new XmlLocator(document, positions);
            XmlPosition result = locator.Inspect(testPosition);

            Assert.NotNull(result);
            Assert.Equal(expectedNodeKind, result.Node.Kind);
            Assert.True(result.IsElementContent, "IsElementContent");

            // TODO: Verify Parent, PreviousSibling, and NextSibling.
        }

        /// <summary>
        ///     Verify that the target line and column lie within an empty element's name.
        /// </summary>
        /// <param name="testFileName">
        ///     The name of the test file, without the extension.
        /// </param>
        /// <param name="line">
        ///     The target line.
        /// </param>
        /// <param name="column">
        ///     The target column.
        /// </param>
        /// <param name="expectedNodeKind">
        ///     The kind of node expected at the position.
        /// </param>
        [InlineData("Test2", 11, 10, "PackageReference")]
        [InlineData("Test2", 12, 18, "PackageReference")]
        [Theory(DisplayName = "Expect line and column to be within empty element's name ")]
        public void Line_Col_InEmptyElementName(string testFileName, int line, int column, string expectedElementName)
        {
            Position testPosition = new Position(line, column);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax document = Parser.ParseText(testXml);

            XmlLocator locator = new XmlLocator(document, positions);
            XmlPosition result = locator.Inspect(testPosition);

            Assert.NotNull(result);
            Assert.Equal(XSNodeKind.Element, result.Node.Kind);
            Assert.True(result.IsElement, "IsElement");

            XSElement element = (XSElement)result.Node;
            Assert.Equal(expectedElementName, element.Name);

            Assert.True(result.IsEmptyElement, "IsEmptyElement");
            Assert.True(result.IsName, "IsName");

            Assert.False(result.IsElementContent, "IsElementContent");

            // TODO: Verify Parent, PreviousSibling, and NextSibling.
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
