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
        ///     A property (<see cref="ProjectProperty"/>) in an MSBuild project.
        /// </summary>
        Property = 3,

        /// <summary>
        ///     An undefined property (<see cref="ProjectPropertyElement"/> without a corresponding <see cref="ProjectProperty"/>) in an MSBuild project.
        /// </summary>
        UndefinedProperty = 4,

        /// <summary>
        ///     A project import (<see cref="ResolvedImport"/>) in an MSBuild project.
        /// </summary>
        Import = 5,

        /// <summary>
        ///     An SDK-style project import (<see cref="ResolvedImport"/>) in an MSBuild project.
        /// </summary>
        SdkImport = 6
    }
}
