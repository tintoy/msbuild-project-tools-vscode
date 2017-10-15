namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     <see cref="XSPath"/>s representing well-known elements in MSBuild projects.
    /// </summary>
    public static class WellKnownElementPaths
    {
        /// <summary>
        ///     The absolute path of the root "Project" element.
        /// </summary>
        public static XSPath Project = XSPath.Parse("/Project");

        /// <summary>
        ///     The relative path that represents a "PropertyGroup" element (static or dynamic).
        /// </summary>
        public static readonly XSPath PropertyGroup = XSPath.Parse("PropertyGroup");

        /// <summary>
        ///     The relative path that represents a "ItemGroup" element (static or dynamic).
        /// </summary>
        public static readonly XSPath ItemGroup = XSPath.Parse("ItemGroup");

        /// <summary>
        ///     The relative path that represents any direct child of an "ItemGroup" element (static or dynamic).
        /// </summary>
        public static readonly XSPath Item = ItemGroup + "*";

        /// <summary>
        ///     The relative path that represents a "PackageReference" item element.
        /// </summary>
        public static readonly XSPath PackageReference = ItemGroup + "PackageReference";

        /// <summary>
        ///     The relative path that represents a "DotNetCliToolReference" item element.
        /// </summary>
        public static readonly XSPath DotNetCliToolReference = ItemGroup + "DotNetCliToolReference";

        /// <summary>
        ///     The absolute path that represents a "Target" element.
        /// </summary>
        public static readonly XSPath Target = Project + "Target";
    }
}
