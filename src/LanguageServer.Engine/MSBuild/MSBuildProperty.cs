using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    /// <summary>
    ///     A property in an MSBuild project.
    /// </summary>
    public sealed class MSBuildProperty
        : MSBuildObject<ProjectProperty>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildProperty"/>.
        /// </summary>
        /// <param name="target">
        ///     The underlying MSBuild <see cref="ProjectProperty"/>.
        /// </param>
        /// <param name="element">
        ///     An <see cref="XmlElementSyntax"/> representing the property's XML element.
        /// </param>
        /// <param name="xmlRange">
        ///     A <see cref="Range"/> representing the span of the property's XML element.
        /// </param>
        public MSBuildProperty(ProjectProperty property, XmlElementSyntaxBase propertyElement, Range xmlRange)
            : base(property, propertyElement, xmlRange)
        {
        }

        /// <summary>
        ///     The property name.
        /// </summary>
        public override string Name => Property.Name;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildProperty"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.Property;

        /// <summary>
        ///     The full path of the file where the target is declared.
        /// </summary>
        public override string SourceFile => Property.Xml.Location.File;

        /// <summary>
        ///     The property's evaluated value.
        /// </summary>
        public string Value => Property.EvaluatedValue;

        /// <summary>
        ///     The property's raw (unevaluated) value.
        /// </summary>
        public string RawValue => Property.UnevaluatedValue;

        /// <summary>
        ///     The underlying MSBuild <see cref="ProjectProperty"/>.
        /// </summary>
        public ProjectProperty Property => UnderlyingObject;
    }
}
