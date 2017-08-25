namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Well-known types of XML nodes in the semantic model.
    /// </summary>
    public enum XSNodeKind
    {
        /// <summary>
        ///     An unknown node type.
        /// </summary>
        /// <remarks>
        ///     Used to detect uninitialised values; do not use directly.
        /// </remarks>
        Unknown = 0,

        /// <summary>
        ///     An XML element.
        /// </summary>
        Element = 1,

        /// <summary>
        ///     An XML attribute.
        /// </summary>
        Attribute = 2,

        /// <summary>
        ///     Text content.
        /// </summary>
        Text = 3,

        /// <summary>
        ///     Non-significant whitespace (the syntax model calls this whitespace trivia).
        /// </summary>
        Whitespace = 4
    }
}
