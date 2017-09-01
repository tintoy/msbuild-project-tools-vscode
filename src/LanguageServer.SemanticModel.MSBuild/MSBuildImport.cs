using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     An import in an MSBuild project.
    /// </summary>
    public class MSBuildImport
        : MSBuildObject<IReadOnlyList<ResolvedImport>>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildImport"/>.
        /// </summary>
        /// <param name="imports">
        ///     The underlying MSBuild <see cref="ResolvedImport"/>.
        /// </param>
        /// <param name="importElement">
        ///     An <see cref="XSElement"/> representing the import's XML element.
        /// </param>
        public MSBuildImport(IReadOnlyList<ResolvedImport> imports, XSElement importElement)
            : base(imports, importElement)
        {
        }

        /// <summary>
        ///     The import name.
        /// </summary>
        public override string Name => Imports[0].ImportingElement.Project;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildImport"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.Import;

        /// <summary>
        ///     The full path of the file where the import is declared.
        /// </summary>
        public override string SourceFile => Imports[0].ImportingElement.Location.File;

        /// <summary>
        ///     The underlying <see cref="ResolvedImport"/>.
        /// </summary>
        public IReadOnlyList<ResolvedImport> Imports => UnderlyingObject;

        /// <summary>
        ///     The import's declaring element.
        /// </summary>
        public XSElement Element => (XSElement)Xml;

        /// <summary>
        ///     The import's "Project" attribute.
        /// </summary>
        public XSAttribute ProjectAttribute => Element["Project"];

        /// <summary>
        ///     The underlying <see cref="ProjectImportElement"/>.
        /// </summary>
        public ProjectImportElement ImportingElement => Imports[0].ImportingElement;

        /// <summary>
        ///     The imported project file names (only returns imported projects that have file names).
        /// </summary>
        public IEnumerable<string> ImportedProjectFiles => Imports.Select(import => import.ImportedProject.ProjectFileLocation.File).Where(projectFile => projectFile != String.Empty);

        /// <summary>
        ///     The imported <see cref="ProjectRootElement"/>.
        /// </summary>
        public IEnumerable<ProjectRootElement> ImportedProjectRoots => Imports.Select(import => import.ImportedProject);
    }
}
