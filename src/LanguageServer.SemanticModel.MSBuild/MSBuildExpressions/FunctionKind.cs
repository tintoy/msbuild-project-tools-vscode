namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Well-known kinds of function.
    /// </summary>
    public enum FunctionKind
    {
        /// <summary>
        ///     A global function, "A(B,C)".
        /// </summary>
        Global,

        /// <summary>
        ///     An instance method, "A.B(C,D)".
        /// </summary>
        InstanceMethod,

        /// <summary>
        ///     A static method, "[A]::B(C,D)".
        /// </summary>
        StaticMethod
    }
}
