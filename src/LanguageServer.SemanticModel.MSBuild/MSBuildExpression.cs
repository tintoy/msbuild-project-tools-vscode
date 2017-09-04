using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using MSBuildExpressions;

    /// <summary>
    ///     Helper methods for working with MSBuild expressions.
    /// /// </summary>
    public static class MSBuildExpression
    {
        /// <summary>
        ///     Parse a generic list.
        /// </summary>
        /// <param name="expression">
        ///     The MSBuild expression to parse.
        /// </param>
        /// <returns>
        ///     A list of <see cref="ExpressionNode"/>s representing the list items.
        /// </returns>
        public static List<ExpressionNode> ParseGenericList(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            List<ExpressionNode> items = new List<ExpressionNode>();

            var parseResult = Parsers.GenericList.TryParse(expression);
            if (parseResult.WasSuccessful)
            {
                items.AddRange(
                    parseResult.Value.Children.Where(node => node.Kind == ExpressionNodeKind.ListItem)
                );
            }

            return items;
        }
    }
}
