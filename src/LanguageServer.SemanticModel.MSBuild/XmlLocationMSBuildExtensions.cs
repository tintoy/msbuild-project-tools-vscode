using System;
using System.Collections.Generic;
using System.Text;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using MSBuildExpressions;

    /// <summary>
    ///     MSBuild-related extension methods for <see cref="XmlLocation"/>.
    /// </summary>
    public static class XmlLocationMSBuildExtensions
    {
        /// <summary>
        ///     Does the location represent an MSBuild expression?
        /// </summary>
        /// <param name="location">
        ///     The XML location.
        /// </param>
        /// <param name="expression">
        ///     Receives the expression (if any) at the location.
        /// </param>
        /// <param name="expressionRange">
        ///     The <see cref="Range"/> that contains the expression.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the location represents an MSBuild expression; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsExpression(this XmlLocation location, out ExpressionNode expression, out Range expressionRange)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            expression = null;
            expressionRange = Range.Zero;

            string expressionText;
            Position expressionStartPosition;
            if (location.IsElementText(out XSElementText text))
            {
                expressionText = text.Text;
                expressionStartPosition = text.Range.Start;
            }
            else if (location.IsAttributeValue(out XSAttribute attribute))
            {
                expressionText = attribute.Value;
                expressionStartPosition = attribute.ValueRange.Start;
            }
            else if (location.IsWhitespace(out XSWhitespace whitespace))
            {
                expressionText = String.Empty;
                expressionStartPosition = whitespace.Range.Start;
            }
            else
                return false;

            ExpressionTree expressionTree;
            if (!MSBuildExpression.TryParse(expressionText, out expressionTree))
                return false;

            Position expressionPosition = location.Position.RelativeTo(expressionStartPosition);

            ExpressionNode expressionAtPosition = expressionTree.FindDeepestNodeAt(expressionPosition);
            if (expressionAtPosition == null)
                return false;

            expression = expressionAtPosition;
            expressionRange = expressionAtPosition.Range.WithOrigin(expressionStartPosition);

            return true;
        }
    }
}
