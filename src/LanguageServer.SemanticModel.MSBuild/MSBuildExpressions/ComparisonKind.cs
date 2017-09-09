namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents a kind of MSBuild comparison expression.
    /// </summary>
    public enum ComparisonKind
    {
        /// <summary>
        ///     Equality ("==").
        /// </summary>
        Equality,

        /// <summary>
        ///     Inequality ("!=").
        /// </summary>
        Inequality
    }
}
