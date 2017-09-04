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
        ///     A <see cref="GenericList"/> node representing the list and its items.
        /// </returns>
        public static GenericList ParseGenericList(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var parseResult = Parsers.GenericList.TryParse(expression);
            if (!parseResult.WasSuccessful)
            {
                throw new ParseException(
                    String.Format("Failed to parse generic list ({0})",
                        parseResult.Expectations.FirstOrDefault() ?? "unknown error"
                    )
                );
            }

            return parseResult.Value.EnsureRelationships();
        }
    }
}
