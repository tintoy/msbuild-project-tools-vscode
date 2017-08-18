namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    /// <summary>
    ///     A type of MSBuild object.
    /// </summary>
    public enum MSBuildObjectKind
    {
        /// <summary>
        ///     An object in an invalid MSBuild project.
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     A target (<see cref="ProjectTargetElement"/>) in an MSBuild project.
        /// </summary>
        Target = 1,

        /// <summary>
        ///     An item (<see cref="ProjectItem"/>) in an MSBuild project.
        /// </summary>
        Item = 2,

        /// <summary>
        ///     A property (<see cref="ProjectProperty"/>) in an MSBuild project.
        /// </summary>
        Property = 3
    }
}
