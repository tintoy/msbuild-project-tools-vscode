using Microsoft.Build.Construction;
using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.MSBuild
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
        ///     An <see cref="XmlElementSyntax"/> representing the target's XML element.
        /// </param>
        /// <param name="xmlRange">
        ///     A <see cref="Range"/> representing the span of the target's XML element.
        /// </param>
        public MSBuildTarget(ProjectTargetElement target, XmlElementSyntaxBase element, Range xmlRange)
            : base(target, element, xmlRange)
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
        ///     The underlying MSBuild <see cref="ProjectTargetElement"/>.
        /// </summary>
        public ProjectTargetElement Target => UnderlyingObject;

    }
}
