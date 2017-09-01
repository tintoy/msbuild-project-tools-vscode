using Microsoft.Build.Construction;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     A target in an MSBuild project.
    /// </summary>
    public sealed class MSBuildTarget
        : MSBuildObject<ProjectTargetElement>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildTarget"/>.
        /// </summary>
        /// <param name="target">
        ///     The underlying MSBuild <see cref="ProjectTargetElement"/>.
        /// </param>
        /// <param name="element">
        ///     An <see cref="XSElement"/> representing the target's XML element.
        /// </param>
        public MSBuildTarget(ProjectTargetElement target, XSElement element)
            : base(target, element)
        {
        }

        /// <summary>
        ///     The target name.
        /// </summary>
        public override string Name => Target.Name;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildTarget"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.Target;

        /// <summary>
        ///     The full path of the file where the target is declared.
        /// </summary>
        public override string SourceFile => Target.Location.File;

        /// <summary>
        ///     The target's declaring element.
        /// </summary>
        public XSElement Element => (XSElement)Xml;

        /// <summary>
        ///     The underlying MSBuild <see cref="ProjectTargetElement"/>.
        /// </summary>
        public ProjectTargetElement Target => UnderlyingObject;

    }
}
