using Microsoft.Build.Construction;
using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Extension methods for working with MSBuild models.
    /// </summary>
    public static class MSBuildModelExtensions
    {
        /// <summary>
        ///     Convert the MSBuild <see cref="ElementLocation"/> to its native equivalent.
        /// </summary>
        /// <param name="location">
        ///     The <see cref="ElementLocation"/> to convert.
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Position"/>.
        /// </returns>
        public static Position ToNative(this ElementLocation location)
        {
            if (location == null)
                return null;

            if (location.Line == 0)
                return Position.Invalid;

            return new Position(location.Line, location.Column);
        }

        /// <summary>
        ///     Get the condition (if any) declared on the element or one of its ancestors.
        /// </summary>
        /// <param name="projectElement">
        ///     The element.
        /// </param>
        /// <returns>
        ///     The condition, or an empty string if no condition is present on the element or one of its ancestors.
        /// </returns>
        public static string FindCondition(this ProjectElement projectElement)
        {
            if (projectElement == null)
                throw new ArgumentNullException(nameof(projectElement));

            ProjectElement currentElement = projectElement;
            while (currentElement != null)
            {
                if (!String.IsNullOrWhiteSpace(currentElement.Condition))
                    return currentElement.Condition;

                currentElement = currentElement.Parent;
            }

            return String.Empty;
        }
    }
}
