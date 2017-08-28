using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Helper methods for working with types from System.Xml.
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        ///     Get the <see cref="Range"/> represented by the <see cref="XmlException"/>.
        /// </summary>
        /// <param name="invalidXml">
        ///     The <see cref="XmlException"/>.
        /// </param>
        /// <returns>
        ///     The <see cref="Range"/>.
        /// </returns>
        public static Range GetRange(this XmlException invalidXml)
        {
            if (invalidXml == null)
                throw new ArgumentNullException(nameof(invalidXml));

            Position startPosition = new Position(
                invalidXml.LineNumber,
                invalidXml.LinePosition
            );

            return startPosition.ToEmptyRange();
        }
    }
}
