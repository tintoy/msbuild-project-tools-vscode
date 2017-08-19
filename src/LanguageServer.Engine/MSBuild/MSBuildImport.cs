using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    /// <summary>
    ///     An import in an MSBuild project.
    /// </summary>
    public class MSBuildImport
        : MSBuildObject<ResolvedImport>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildImport"/>.
        /// </summary>
        /// <param name="import">
        ///     The underlying MSBuild <see cref="ResolvedImport"/>.
        /// </param>
        /// <param name="importElement">
        ///     An <see cref="XmlElementSyntaxBase"/> representing the import's XML element.
        /// </param>
        /// <param name="xmlRange">
        ///     A <see cref="Range"/> representing the span of the item's XML element.
        /// </param>
        public MSBuildImport(ResolvedImport import, XmlElementSyntaxBase importElement, Range xmlRange)
            : base(import, importElement, xmlRange)
        {
        }

        /// <summary>
        ///     The import name.s
        /// </summary>
        public override string Name => Import.ImportingElement.Project;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildImport"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.Import;

        /// <summary>
        ///     The full path of the file where the import is declared.
        /// </summary>
        public override string SourceFile => Import.ImportingElement.Location.File;

        /// <summary>
        ///     The underlying <see cref="ResolvedImport"/>.
        /// </summary>
        public ResolvedImport Import => UnderlyingObject;

        /// <summary>
        ///     The import's "Project" attribute.
        /// </summary>
        public XmlAttributeSyntax ProjectAttribute => ((XmlElementSyntaxBase)Xml).AsSyntaxElement["Project"];

        /// <summary>
        ///     The underlying <see cref="Microsoft.Build.Construction.ProjectImportElement"/>.
        /// </summary>
        public ProjectImportElement ProjectImportElement => Import.ImportingElement;

        /// <summary>
        ///     The imported <see cref="ProjectRootElement"/>.
        /// </summary>
        public ProjectRootElement ImportedProjectRoot => Import.ImportedProject;
    }
}
