namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Well-known kinds of MSBuild expression nodes.
    /// </summary>
    public enum ExpressionKind
    {
        /// <summary>
        ///     A semicolon-delimited list.
        /// </summary>
        List,

        /// <summary>
        ///     A generic list item.
        /// </summary>
        ListItem,

        /// <summary>
        ///     A generic list item separator.
        /// </summary>
        ListSeparator,

        /// <summary>
        ///     A quoted string.
        /// </summary>
        QuotedString
    }
}
