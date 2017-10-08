using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Helper methods for working with types from System.Xml.
    /// </summary>
    public static class XmlExceptionExtensions
    {
        /// <summary>
        ///     Get the <see cref="Range"/> represented by the <see cref="XmlException"/>.
        /// </summary>
        /// <param name="invalidXml">
        ///     The <see cref="XmlException"/>.
        /// </param>
        /// <param name="xmlLocator">
        ///     The XML locator API (if available).
        /// </param>
        /// <returns>
        ///     The <see cref="Range"/>.
        /// </returns>
        public static Range GetRange(this XmlException invalidXml, XmlLocator xmlLocator)
        {
            if (invalidXml == null)
                throw new ArgumentNullException(nameof(invalidXml));

            Position startPosition = new Position(
                invalidXml.LineNumber,
                invalidXml.LinePosition
            );

            // Attempt to use the range of the actual XML that the exception refers to.
            XmlLocation location = xmlLocator?.Inspect(startPosition);
            if (location != null)
                return location.Node.Range;

            // Otherwise, just use the start position.
            return startPosition.ToEmptyRange();
        }
    }
}
