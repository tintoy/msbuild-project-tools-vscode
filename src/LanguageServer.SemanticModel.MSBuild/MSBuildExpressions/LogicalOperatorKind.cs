namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents a kind of MSBuild logical operator.
    /// </summary>
    public enum LogicalOperatorKind
    {
        /// <summary>
        ///     Logical-AND.
        /// </summary>
        And,

        /// <summary>
        ///     Logical-OR.
        /// </summary>
        Or,

        /// <summary>
        ///     Logical-NOT.
        /// </summary>
        Not
    }
}
