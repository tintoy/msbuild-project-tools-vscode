

using System;
using Microsoft.Build.Exceptions;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Extension methods for MSBuild-related exceptions.
    /// </summary>
    public static class MSBuildExceptionExtensions
    {
        /// <summary>
        ///     Get the <see cref="Range"/> represented by the <see cref="InvalidProjectFileException"/>.
        /// </summary>
        /// <param name="invalidProjectFileException">
        ///     The <see cref="InvalidProjectFileException"/>.
        /// </param>
        /// <param name="xmlLocator">
        ///     The XML locator API (if available).
        /// </param>
        /// <returns>
        ///     The <see cref="Range"/>.
        /// </returns>
        public static Range GetRange(this InvalidProjectFileException invalidProjectFileException, XmlLocator xmlLocator)
        {
            if (invalidProjectFileException == null)
                throw new ArgumentNullException(nameof(invalidProjectFileException));

            Position startPosition = new Position(
                invalidProjectFileException.LineNumber,
                invalidProjectFileException.ColumnNumber
            );

            // Attempt to use the range of the actual XML that the exception refers to.
            XmlLocation location = xmlLocator?.Inspect(startPosition);
            if (location != null)
                return location.Node.Range;

            // Otherwise, fall back to using the exception's declared end position...
            Position endPosition = new Position(
                invalidProjectFileException.EndLineNumber,
                invalidProjectFileException.EndColumnNumber
            );

            // ...although it's sometimes less reliable.
            if (endPosition == Position.Zero)
                endPosition = startPosition;

            return new Range(startPosition, endPosition);
        }
    }
}
