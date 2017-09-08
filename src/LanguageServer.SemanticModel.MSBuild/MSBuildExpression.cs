using Sprache;
using System;
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
        ///     Parse an MSBuild expression.
        /// </summary>
        /// <param name="expression">
        ///     the expression to parse.
        /// </param>
        /// <returns>
        ///     An <see cref="ExpressionNode"/> representing the root of the expression tree.
        /// </returns>
        public static ExpressionNode Parse(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var parseResult = Parsers.Root.TryParse(expression);
            if (!parseResult.WasSuccessful)
            {
                string expectations = String.Empty;
                if (parseResult.Expectations.Any())
                {
                    expectations = String.Format(" (expected: [{0}])",
                        String.Join(", ", parseResult.Expectations.Select(
                            expectation => String.Format("'{0}'", expectation)
                        ))
                    );
                }

                throw new ParseException(
                    String.Format("Failed to parse expression{0}", expectations)
                );
            }

            return parseResult.Value.EnsureRelationships();
        }

        /// <summary>
        ///     Parse a simple list.
        /// </summary>
        /// <param name="expression">
        ///     The MSBuild expression to parse.
        /// </param>
        /// <returns>
        ///     A <see cref="SimpleList"/> node representing the list and its items.
        /// </returns>
        public static SimpleList ParseSimpleList(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var parseResult = Parsers.SimpleLists.List.TryParse(expression);
            if (!parseResult.WasSuccessful)
            {
                throw new ParseException(
                    String.Format("Failed to parse simple list ({0})",
                        parseResult.Expectations.FirstOrDefault() ?? "unknown error"
                    )
                );
            }

            return parseResult.Value.EnsureRelationships();
        }
    }
}
