using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace MSBuildProjectTools.LanguageServer.Tests
{
    using XmlParser;
    
    /// <summary>
    ///     Tests for <see cref="PositionalObjectLookup"/>.
    /// </summary>
    public class PositionalNodeLookupTests
    {
        /// <summary>
        ///     Verify that an element can be found by position.
        /// </summary>
        [Fact]
        public void Can_find_element_by_position()
        {
            const string xml = @"
<node1>
    <node2 attribute1=""foo"" attribute2=""bar"" />
    <node3 attribute1=""baz"" attribute2=""bonk"" />
</node1>
";

            XDocument document;
            using (StringReader reader = new StringReader(xml))
            {
                document = Parser.Load(reader);
            }

            PositionalObjectLookup lookup = new PositionalObjectLookup(document);

            // node1/node2
            XObject match = lookup.Find(new Position(
                lineNumber: 3, // First line is blank
                columnNumber: 6
            ));
            Assert.NotNull(match);
            Assert.IsType<XElement>(match);

            XElement matchingElement = (XElement)match;
            Assert.Equal("node2", matchingElement.Name);
        }

        /// <summary>
        ///     Verify that an attribute can be found by position.
        /// </summary>
        [Fact]
        public void Can_find_attribute_by_position()
        {
            const string xml = @"
<node1>
    <node2 attribute1=""foo"" attribute2=""bar"" />
    <node3 attribute1=""baz"" attribute2=""bonk"" />
</node1>
";

            XDocument document;
            using (StringReader reader = new StringReader(xml))
            {
                document = Parser.Load(reader);
            }

            PositionalObjectLookup lookup = new PositionalObjectLookup(document);

            // node1/node3/@attribute2
            XObject match = lookup.Find(new Position(
                lineNumber: 4, // First line is blank
                columnNumber: 33
            ));
            Assert.NotNull(match);
            Assert.IsType<XAttribute>(match);

            XAttribute matchingAttribute = (XAttribute)match;
            Assert.Equal("attribute2", matchingAttribute.Name);
        }

        /// <summary>
        ///     Verify that a PackageReference element can be found by position.
        /// </summary>
        [Fact]
        public void Can_find_PackageReference_element_by_position()
        {
            const string xml = @"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard1.3</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Newtonsoft.Json""  Version=""10.0.3"" />
    </ItemGroup>
</Project>
";

            XDocument document;
            using (StringReader reader = new StringReader(xml))
            {
                document = Parser.Load(reader);
            }

            PositionalObjectLookup lookup = new PositionalObjectLookup(document);

            // Project/ItemGroup/PackageReference
            XObject match = lookup.Find(new Position(
                lineNumber: 8, // First line is blank
                columnNumber: 12
            ));
            Assert.NotNull(match);
            Assert.IsType<XElement>(match);

            XElement matchingElement = (XElement)match;
            Assert.Equal("PackageReference", matchingElement.Name);
        }
    }
}
