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

            var parseResult = Parsers.SimpleList.TryParse(expression);
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
