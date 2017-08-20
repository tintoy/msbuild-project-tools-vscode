using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

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
        ///     An item (<see cref="ProjectItem"/>) in an MSBuild project whose condition evaluates as <c>false</c>.
        /// </summary>
        UnusedItem = 3,

        /// <summary>
        ///     A property (<see cref="ProjectProperty"/>) in an MSBuild project.
        /// </summary>
        Property = 4,

        /// <summary>
        ///     An unused property (<see cref="ProjectPropertyElement"/> without a corresponding <see cref="ProjectProperty"/>) in an MSBuild project.
        /// </summary>
        UnusedProperty = 5,

        /// <summary>
        ///     A project import (<see cref="ResolvedImport"/>) in an MSBuild project.
        /// </summary>
        Import = 6,

        /// <summary>
        ///     An unresolved import (<see cref="ProjectImportElement"/> without a corresponding <see cref="ResolvedImport"/>) in an MSBuild project.
        /// </summary>
        UnresolvedImport = 7,

        /// <summary>
        ///     An SDK-style project import (<see cref="ResolvedImport"/>) in an MSBuild project.
        /// </summary>
        SdkImport = 8
    }
}
