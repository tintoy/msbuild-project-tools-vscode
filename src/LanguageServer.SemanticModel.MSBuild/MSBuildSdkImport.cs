using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     An SDK-style import in an MSBuild project.
    /// </summary>
    public class MSBuildSdkImport
        : MSBuildObject<IReadOnlyList<ResolvedImport>>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildImport"/>.
        /// </summary>
        /// <param name="imports">
        ///     A read-only list of underlying MSBuild <see cref="ResolvedImport"/>s representing the imports resulting from the SDK import.
        /// </param>
        /// <param name="sdkAttribute">
        ///     An <see cref="XSAttribute"/> representing the import's "Sdk" attribute.
        /// </param>
        public MSBuildSdkImport(IReadOnlyList<ResolvedImport> imports, XSAttribute sdkAttribute)
            : base(imports, sdkAttribute)
        {
        }

        /// <summary>
        ///     The import name.
        /// </summary>
        public override string Name => Imports[0].ImportingElement.Sdk;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildImport"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.SdkImport;

        /// <summary>
        ///     The full path of the file where the import is declared.
        /// </summary>
        public override string SourceFile => Imports[0].ImportingElement.Location.File;

        /// <summary>
        ///     The import's "Sdk" attribute.
        /// </summary>
        public XSAttribute Attribute => (XSAttribute)Xml;

        /// <summary>
        ///     The underlying <see cref="ResolvedImport"/>s.
        /// </summary>
        public IReadOnlyList<ResolvedImport> Imports => UnderlyingObject;

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
