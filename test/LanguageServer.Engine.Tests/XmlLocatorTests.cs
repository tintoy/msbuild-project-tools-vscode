using Microsoft.Language.Xml;
using System;
using System.IO;
using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests
{
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
        /// <param name="line">
        ///     The target line.
        /// </param>
        /// <param name="column">
        ///     The target column.
        /// </param>
        [Theory]
        [InlineData(2, 5)]
        [InlineData(3, 9)]
        public void Line_Col_ListInsideElement1(int line, int column)
        {
            Position testPosition = new Position(line, column);
            Console.WriteLine("Test Position: {0}", testPosition);

            string testXml = LoadTestFile("TestFiles", "Test1.xml");
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
        ///     Verify that the target line and column lie after an element inside element 2.
        /// </summary>
        /// <param name="line">
        ///     The target line.
        /// </param>
        /// <param name="column">
        ///     The target column.
        /// </param>
        [Theory]
        [InlineData(3, 21)]
        public void Line_Col_AfterElement(int line, int column)
        {
            string testXml = LoadTestFile("TestFiles", "Test1.xml");
            TextPositions positions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            Position testPosition = new Position(line, column);
            int absolutePosition = positions.GetAbsolutePosition(testPosition);
            SyntaxNode foundNode = xmlDocument.FindNode(absolutePosition,
                descendIntoChildren: node => true
            );
            Assert.NotNull(foundNode);

            Range nodeSpan = foundNode.FullSpan.ToNative(positions);
            Assert.True(nodeSpan.Contains(testPosition),
                $"Test position {testPosition} must lie within {foundNode.Kind} full-span {nodeSpan}."
            );

            XmlElementSyntaxBase element = foundNode.GetContainingElement();
            Assert.NotNull(element);

            Assert.Equal("Element4", element.Name);

            Range elementSpan = element.Span.ToNative(positions);
            Assert.True(elementSpan.Contains(testPosition),
                $"Test position {testPosition} must lie after {element.Name} span {elementSpan}."
            );

            Assert.Equal("Element1", element.Name);
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
