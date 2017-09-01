using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using Utilities;

    /// <summary>
    ///     An unresolved SDK-style import in an MSBuild project.
    /// </summary>
    public class MSBuildUnresolvedSdkImport
        : MSBuildObject<ProjectImportElement>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildUnresolvedSdkImport"/>.
        /// </summary>
        /// <param name="import">
        ///     The underlying MSBuild <see cref="ProjectImportElement"/>.
        /// </param>
        /// <param name="sdkAttribute">
        ///     An <see cref="XSAttribute"/> representing the import's "Sdk" attribute.
        /// </param>
        public MSBuildUnresolvedSdkImport(ProjectImportElement import, XSAttribute sdkAttribute)
            : base(import, sdkAttribute)
        {
        }

        /// <summary>
        ///     The import name.
        /// </summary>
        public override string Name => ImportingElement.Project;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildUnresolvedSdkImport"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.UnresolvedSdkImport;

        /// <summary>
        ///     The full path of the file where the import is declared.
        /// </summary>
        public override string SourceFile => ImportingElement.Location.File;

        /// <summary>
        ///     The imported SDK.
        /// </summary>
        public string Sdk => ImportingElement.Sdk;

        /// <summary>
        ///     The unresolved item's unevaluated condition.
        /// </summary>
        public string Condition => ImportingElement.FindCondition();

        /// <summary>
        ///     The import's "Sdk" attribute.
        /// </summary>
        public XSAttribute SdkAttribute => (XSAttribute)Xml;

        /// <summary>
        ///     The underlying <see cref="ProjectImportElement"/>.
        /// </summary>
        public ProjectImportElement ImportingElement => UnderlyingObject;
    }
}
