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
        [Theory(DisplayName = "Inside element ")]
        public void InsideElement1(string testFileName, int line, int column)
        {
            // TODO: Change this test to use XmlLocator.

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
        [Theory(DisplayName = "Within element content ")]
        public void InElementContent(string testFileName, int line, int column, XSNodeKind expectedNodeKind)
        {
            Position testPosition = new Position(line, column);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax document = Parser.ParseText(testXml);

            XmlLocator locator = new XmlLocator(document, positions);
            XmlLocation result = locator.Inspect(testPosition);

            Assert.NotNull(result);
            Assert.Equal(expectedNodeKind, result.Node.Kind);
            Assert.True(result.IsElementContent(), "IsElementContent");

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
        /// <param name="expectedElementName">
        ///     The expected element name.
        /// </param>
        [InlineData("Test2", 11, 10, "PackageReference")]
        [InlineData("Test2", 12, 18, "PackageReference")]
        [Theory(DisplayName = "Within empty element's name ")]
        public void InEmptyElementName(string testFileName, int line, int column, string expectedElementName)
        {
            Position testPosition = new Position(line, column);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax document = Parser.ParseText(testXml);

            XmlLocator locator = new XmlLocator(document, positions);
            XmlLocation result = locator.Inspect(testPosition);

            Assert.NotNull(result);
            Assert.Equal(XSNodeKind.Element, result.Node.Kind);
            Assert.True(result.IsElement(), "IsElement");

            XSElement element = (XSElement)result.Node;
            Assert.Equal(expectedElementName, element.Name);

            Assert.True(result.IsEmptyElement(), "IsEmptyElement");
            Assert.True(result.IsName(), "IsName");

            Assert.False(result.IsElementContent(), "IsElementContent");

            // TODO: Verify Parent, PreviousSibling, and NextSibling.
        }

        /// <summary>
        ///     Verify that the target line and column lie within an attribute's value (excluding the enclosing quotes).
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
        /// <param name="expectedAttributeName">
        ///     The expected attribute name.
        /// </param>
        [InlineData("Test2", 11, 36, "Include")]
        [InlineData("Test2", 11, 37, "Include")]
        [InlineData("Test2", 11, 51, "Include")]
        [InlineData("Test2", 11, 62, "Version")]
        [InlineData("Test2", 11, 63, "Version")]
        [InlineData("Test2", 11, 68, "Version")]
        [Theory(DisplayName = "Within attribute's value ")]
        public void InAttributeValue(string testFileName, int line, int column, string expectedAttributeName)
        {
            Position testPosition = new Position(line, column);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax document = Parser.ParseText(testXml);

            XmlLocator locator = new XmlLocator(document, positions);
            XmlLocation result = locator.Inspect(testPosition);        
            Assert.NotNull(result);

            XSAttribute attribute;
            Assert.True(result.IsAttribute(out attribute), "IsAttribute");
            Assert.True(result.IsAttributeValue(), "IsAttributeValue");

            Assert.Equal(expectedAttributeName, attribute.Name);

            // TODO: Verify Parent, PreviousSibling, and NextSibling.
        }

        /// <summary>
        ///     Verify that the target line and column are on an element that can be replaced by completion.
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
        [InlineData("Invalid1.DoubleOpeningTag", 4, 10)]
        [InlineData("Invalid1.EmptyOpeningTag", 5, 10)]
        [InlineData("Invalid2.DoubleOpeningTag", 13, 10)]
        [InlineData("Invalid2.EmptyOpeningTag", 13, 65)]
        [InlineData("Invalid2.NoClosingTag", 14, 10)]
        [Theory(DisplayName = "On element that can be replaced by completion ")]
        public void CanCompleteElement(string testFileName, int line, int column)
        {
            Position testPosition = new Position(line, column);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax document = Parser.ParseText(testXml);

            XmlLocator locator = new XmlLocator(document, positions);
            XmlLocation location = locator.Inspect(testPosition);
            Assert.NotNull(location);

            XSElement replacingElement;
            Assert.True(location.CanCompleteElement(out replacingElement), "CanCompleteReplacement");
            Assert.NotNull(replacingElement);
        }

        /// <summary>
        ///     Verify that the target line and column are on an element that can be replaced by completion.
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
        [InlineData("Invalid1.DoubleOpeningTag", 4, 10, "Element2")]
        [InlineData("Invalid1.EmptyOpeningTag", 5, 10, "Element2")]
        [InlineData("Invalid2.DoubleOpeningTag", 13, 10, "ItemGroup")]
        [InlineData("Invalid2.EmptyOpeningTag", 13, 65, "ItemGroup")]
        [InlineData("Invalid2.NoClosingTag", 10, 5, "Project")]
        [Theory(DisplayName = "On completable element if parent name matches ")]
        public void CanCompleteElementInParentNamed(string testFileName, int line, int column, string expectedParent)
        {
            Position testPosition = new Position(line, column);

            string testXml = LoadTestFile("TestFiles", testFileName + ".xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax document = Parser.ParseText(testXml);

            XmlLocator locator = new XmlLocator(document, positions);
            XmlLocation location = locator.Inspect(testPosition);
            Assert.NotNull(location);

            XSElement replaceElement;
            Assert.True(
                location.CanCompleteElement(out replaceElement, asChildOfElementNamed: expectedParent),
                "CanCompleteReplacement"
            );
            Assert.NotNull(replaceElement);
            Assert.Equal(expectedParent, replaceElement.ParentElement?.Name);
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
