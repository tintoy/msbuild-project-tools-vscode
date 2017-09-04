using Sprache;
using System.Collections.Generic;
using Xunit;
using System;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.Tests.ParserTests
{
    using SemanticModel.MSBuildExpressions;

    /// <summary>
    ///     Tests for parsing of MSBuild list expressions.
    /// </summary>
    public class ListParserTests
    {
        /// <summary>
        ///     Verify that the <see cref="Parsers.GenericList"/> MSBuild expression parser can parse a simple generic list.
        /// </summary>
        [Fact(DisplayName = "Parse MSBuild generic list")] // TODO: Reimplement as theory.
        public void ParseGenericList()
        {
            AssertParser.SucceedsWith(Parsers.GenericList, "ABC;DEF", result =>
            {
                Assert.Equal(ExpressionNodeKind.List, result.Kind);

                Assert.Collection(result.Children,
                    item1 =>
                    {
                        Assert.Equal(ExpressionNodeKind.ListItem, item1.Kind);
                        Assert.Equal("ABC", item1.Value);
                    },
                    item2 =>
                    {
                        Assert.Equal(ExpressionNodeKind.ListItem, item2.Kind);
                        Assert.Equal("DEF", item2.Value);
                    }
                );
            });
        }
    }
}
