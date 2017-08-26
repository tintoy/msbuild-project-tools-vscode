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

        [Fact]
        public void DumpSemanticModel()
        {
            string testXml = LoadTestFile("TestFiles", "Test1.xml");
            TextPositions xmlPositions = new TextPositions(testXml);
            XmlDocumentSyntax xmlDocument = Parser.ParseText(testXml);

            List<XSNode> semanticModel = xmlDocument.GetSemanticModel(xmlPositions);
            Assert.NotNull(semanticModel);
            TestOutput.WriteLine("{0} nodes discovered.", semanticModel.Count);

            foreach (XSNode node in semanticModel)
            {
                string nodeName = "";
                switch (node)
                {
                    case XSElement element:
                    {
                        nodeName = element.Name;

                        break;
                    }
                    case XSAttribute attribute:
                    {
                        nodeName = attribute.Name;

                        break;
                    }
                }

                TestOutput.WriteLine("{0} '{1}' spanning {2}",
                    node.Kind, nodeName, node.Range
                );
            }
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
