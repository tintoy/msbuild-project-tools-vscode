using Microsoft.Build.Evaluation;
using Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    // BUG: This can't handle the fact that multiple items may come from the same XML element.
    // TODO: Change this to store a list of ProjectItems instead.

    /// <summary>
    ///     An item in an MSBuild project.
    /// </summary>
    public sealed class MSBuildItem
        : MSBuildObject<ProjectItem>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildItem"/>.
        /// </summary>
        /// <param name="item">
        ///     The underlying MSBuild <see cref="ProjectItem"/>.
        /// </param>
        /// <param name="element">
        ///     An <see cref="XmlElementSyntax"/> representing the item's XML element.
        /// </param>
        /// <param name="xmlRange">
        ///     A <see cref="Range"/> representing the span of the item's XML element.
        /// </param>
        public MSBuildItem(ProjectItem item, XmlElementSyntaxBase itemElement, Range xmlRange)
            : base(item, itemElement, xmlRange)
        {
        }

        /// <summary>
        ///     The item name.
        /// </summary>
        public override string Name => Item.Xml.ItemType;

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildItem"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.Item;

        /// <summary>
        ///     The full path of the file where the target is declared.
        /// </summary>
        public override string SourceFile => Item.Xml.Location.File;

        /// <summary>
        ///     The evaluated value of the item's "Include" attribute.
        /// </summary>
        public string Include => Item.EvaluatedInclude;

        /// <summary>
        ///     The raw (unevaluated) value of the item's "Include" attribute.
        /// </summary>
        public string RawInclude => Item.UnevaluatedInclude;

        /// <summary>
        ///     The underlying MSBuild <see cref="ProjectItem"/>.
        /// </summary>
        public ProjectItem Item => UnderlyingObject;

        /// <summary>
        ///     Get the evaluated value of the specified item metadata.
        /// </summary>
        /// <param name="name">
        ///     The metadata name.
        /// </param>
        /// <returns>
        ///     The metadata's evaluated value, or <c>null</c> if no metadata was found with the specified name.
        /// </returns>
        /// <remarks>
        ///     Can be used to access built-in metadata (such as "FullPath").
        /// </remarks>
        public string GetMetadataValue(string name) => Item.GetMetadataValue(name);

        /// <summary>
        ///     Get the raw (unevaluated) value of the specified item metadata.
        /// </summary>
        /// <param name="name">
        ///     The metadata name.
        /// </param>
        /// <returns>
        ///     The metadata's evaluated value, or <c>null</c> if no metadata was found with the specified name.
        /// </returns>
        /// <remarks>
        ///     Cannot be used to access built-in metadata (such as "FullPath"); use <see cref="GetMetadataValue"/>, instead.
        /// </remarks>
        public string GetRawMetadataValue(string name) => Item.GetMetadata(name)?.UnevaluatedValue;
    }
}
