using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents an invalid XML attribute.
    /// </summary>
    public class XSInvalidAttribute
        : XSAttribute
    {
        /// <summary>
        ///     Create a new <see cref="XSInvalidAttribute"/>.
        /// </summary>
        /// <param name="attribute">
        ///     The <see cref="XmlAttributeSyntax"/> represented by the <see cref="XSInvalidAttribute"/>.
        /// </param>
        /// <param name="range">
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute.
        /// </param>
        /// <param name="nameRange">
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute's name.
        /// </param>
        /// <param name="valueRange">
        ///     The <see cref="Range"/>, within the source text, spanned by the attribute's value.
        /// </param>
        /// <param name="element">
        ///     The element that contains the attribute.
        /// </param>
        public XSInvalidAttribute(XmlAttributeSyntax attribute, Range range, Range nameRange, Range valueRange, XSElement element)
            : base(attribute, range, nameRange, valueRange, element)
        {
        }

        /// <summary>
        ///     Does the <see cref="XSNode"/> represent valid XML?
        /// </summary>
        public override bool IsValid => false;
    }
}
