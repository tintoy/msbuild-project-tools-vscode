using Sprache;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Parsers for MSBuild expression syntax.
    /// </summary>
    static class Parsers
    {
        /// <summary>
        ///     Parse a generic MSBuild list item.
        /// </summary>
        public static readonly Parser<ExpressionNode> GenericListItem = Parse.Positioned(
            from item in Tokens.ListChar.Many().Text()
            select new ExpressionNode
            {
                Kind = ExpressionNodeKind.ListItem,
                Value = item
            }
        );

        /// <summary>
        ///     Parse a generic MSBuild list, delimited by semicolons.
        /// </summary>
        public static readonly Parser<ExpressionNode> GenericList = Parse.Positioned(
            from items in GenericListItem.Many()
            select new ExpressionNode
            {
                Kind = ExpressionNodeKind.List,
                Children = ImmutableList.CreateRange(items)
            }
        );
    }
}
