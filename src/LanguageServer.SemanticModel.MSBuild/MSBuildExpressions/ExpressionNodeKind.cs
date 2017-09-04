namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Well-known kinds of MSBuild expression nodes.
    /// </summary>
    public enum ExpressionNodeKind
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
        ///     A quoted string.
        /// </summary>
        QuotedString
    }
}
