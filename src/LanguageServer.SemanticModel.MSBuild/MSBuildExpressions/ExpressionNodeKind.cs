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
        ///     A simple list item.
        /// </summary>
        ListItem,

        /// <summary>
        ///     A simple list item separator.
        /// </summary>
        ListSeparator,

        /// <summary>
        ///     A quoted string.
        /// </summary>
        QuotedString
    }
}
