using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using Utilities;

    /// <summary>
    ///     An unresolved regular-style import in an MSBuild project.
    /// </summary>
    public class MSBuildUnresolvedImport
        : MSBuildObject<ProjectImportElement>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildUnresolvedImport"/>.
        /// </summary>
        /// <param name="import">
        ///     The underlying MSBuild <see cref="ProjectImportElement"/>.
        /// </param>
        /// <param name="importElement">
        ///     An <see cref="XSElement"/> representing the import's XML element.
        /// </param>
        public MSBuildUnresolvedImport(ProjectImportElement import, XSElement importElement)
            : base(import, importElement)
        {
        }

        /// <summary>
        ///     The import name.
        /// </summary>
        public override string Name => ImportingElement.Project;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildUnresolvedImport"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.UnresolvedImport;

        /// <summary>
        ///     The full path of the file where the import is declared.
        /// </summary>
        public override string SourceFile => ImportingElement.Location.File;

        /// <summary>
        ///     The import's declaring element.
        /// </summary>
        public XSElement Element => (XSElement)Xml;

        /// <summary>
        ///     The imported path.
        /// </summary>
        public string Project => ImportingElement.Project;

        /// <summary>
        ///     The unresolved item's unevaluated condition.
        /// </summary>
        public string Condition => ImportingElement.FindCondition();

        /// <summary>
        ///     The import's "Project" attribute.
        /// </summary>
        public XSAttribute ProjectAttribute => Element["Project"];

        /// <summary>
        ///     The underlying <see cref="ProjectImportElement"/>.
        /// </summary>
        public ProjectImportElement ImportingElement => UnderlyingObject;
    }
}
