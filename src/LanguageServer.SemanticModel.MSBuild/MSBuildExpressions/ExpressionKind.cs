namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Well-known kinds of MSBuild expression nodes.
    /// </summary>
    public enum ExpressionKind
    {
        /// <summary>
        ///     The root of an expression tree.
        /// </summary>
        Root,

        /// <summary>
        ///     A semicolon-delimited list of simple items.
        /// </summary>
        SimpleList,

        /// <summary>
        ///     A simple list item.
        /// </summary>
        SimpleListItem,

        /// <summary>
        ///     A simple list item separator.
        /// </summary>
        SimpleListSeparator,

        /// <summary>
        ///     A semicolon-delimited list of expressions.
        /// </summary>
        List,

        /// <summary>
        ///     Placeholder representing an empty slot in an expression list.
        /// </summary>
        EmptyListItem,

        /// <summary>
        ///     A quoted string.
        /// </summary>
        QuotedString,

        /// <summary>
        ///     An evaluation expression, "$(xxx)".
        /// </summary>
        Evaluate,

        /// <summary>
        ///     An item group expression, "@(xxx)".
        /// </summary>
        ItemGroup,

        /// <summary>
        ///     A function-call expression, "XXX(A,B,C)".
        /// </summary>
        FunctionCall,

        /// <summary>
        ///     A comparison expression.
        /// </summary>
        Compare,

        /// <summary>
        ///     A logical expression (e.g. AND, OR, NOT).
        /// </summary>
        Logical,

        /// <summary>
        ///     A generic symbol.
        /// </summary>
        Symbol
    }
}
