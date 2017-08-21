using Microsoft.Build.Evaluation;
using Microsoft.Build.Construction;
using Microsoft.Language.Xml;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.MSBuild
{
    /// <summary>
    ///     An item group declaration in an MSBuild project.
    /// </summary>
    /// <remarks>
    ///     An item in an MSBuild project can actually derive from one of several <see cref="MSBuildItemGroup"/>s, depending on where it was declared.
    /// </remarks>
    public sealed class MSBuildItemGroup
        : MSBuildObject<IReadOnlyList<ProjectItem>>
    {
        /// <summary>
        ///     Create a new <see cref="MSBuildItemGroup"/>.
        /// </summary>
        /// <param name="items">
        ///     The underlying MSBuild <see cref="ProjectItem"/>s.
        /// </param>
        /// <param name="originatingElement">
        ///     The MSBuild <see cref="ProjectItemElement"/> from where the items originate.
        /// </param>
        /// <param name="itemElement">
        ///     An <see cref="XmlElementSyntax"/> representing the item's XML element.
        /// </param>
        /// <param name="xmlRange">
        ///     A <see cref="Range"/> representing the span of the item's XML element.
        /// </param>
        public MSBuildItemGroup(IReadOnlyList<ProjectItem> items, ProjectItemElement originatingElement, XmlElementSyntaxBase itemElement, Range xmlRange)
            : base(items, itemElement, xmlRange)
        {
            if (Items.Count == 0)
                throw new ArgumentException("Must specify at least one ProjectItem.", nameof(items));

            if (originatingElement == null)
                throw new ArgumentNullException(nameof(originatingElement));

            Name = itemElement.Name;
            OriginatingElement = originatingElement;
        }

        /// <summary>
        ///     The item group name.
        /// </summary>
        public override string Name { get; }

        /// <summary>
        ///     The kind of MSBuild object represented by the <see cref="MSBuildItemGroup"/>.
        /// </summary>
        public override MSBuildObjectKind Kind => MSBuildObjectKind.Item;

        /// <summary>
        ///     The full path of the file where the target is declared.
        /// </summary>
        public override string SourceFile => OriginatingElement.Location.File;

        /// <summary>
        ///     The evaluated value of the first item's "Include" attribute.
        /// </summary>
        public string FirstInclude => FirstItem.EvaluatedInclude;

        /// <summary>
        ///     The raw (unevaluated) value of the first item's "Include" attribute.
        /// </summary>
        public string FirstRawInclude => OriginatingElement.Include;

        /// <summary>
        ///     The MSBuild <see cref="ProjectItemElement"/> from where the items originate.
        /// </summary>
        public ProjectItemElement OriginatingElement { get; }

        /// <summary>
        ///     The evaluated values of the items' "Include" attributes.
        /// </summary>
        public IEnumerable<string> Includes => Items.Select(item => item.EvaluatedInclude);

        /// <summary>
        ///     The raw (unevaluated) values of the items' "Include" attributes.
        /// </summary>
        public IEnumerable<string> RawIncludes => Items.Select(item => item.UnevaluatedInclude);

        /// <summary>
        ///     The (distinct) names of all metadata defined on the group's items.
        /// </summary>
        public IEnumerable<string> MetadataNames => Items.SelectMany(item => item.Metadata.Select(metadata => metadata.Name)).Distinct();

        /// <summary>
        ///     The underlying MSBuild <see cref="ProjectItem"/>.
        /// </summary>
        public IReadOnlyList<ProjectItem> Items => UnderlyingObject;

        /// <summary>
        ///     Does the item group declaration yield a single item?
        /// </summary>
        public bool HasSingleItem => Items.Count == 1;

        /// <summary>
        ///     The first underlying MSBuild <see cref="ProjectItem"/> in the item group.
        /// </summary>
        public ProjectItem FirstItem => Items[0];

        /// <summary>
        ///     The item group's declaring XML element.
        /// </summary>
        public XmlElementSyntaxBase ItemElement => (XmlElementSyntaxBase)base.Xml;

        /// <summary>
        ///     Get the evaluated values of the specified metadata for the items in the group.
        /// </summary>
        /// <param name="name">
        ///     The metadata name.
        /// </param>
        /// <returns>
        ///     The metadata's evaluated values (individual values can be <c>null</c> for items where no metadata was found with the specified name).
        /// </returns>
        /// <remarks>
        ///     Can be used to access built-in metadata (such as "FullPath").
        /// </remarks>
        public IEnumerable<string> GetMetadataValues(string name) => Items.Select(item => item.GetMetadataValue(name));

        /// <summary>
        ///     Get the raw (unevaluated) values of the specified metadata for the items in the group.
        /// </summary>
        /// <param name="name">
        ///     The metadata name.
        /// </param>
        /// <returns>
        ///     The metadata's unevaluated values (individual values can be <c>null</c> for items where no metadata was found with the specified name).
        /// </returns>
        /// <remarks>
        ///     Can be used to access built-in metadata (such as "FullPath").
        /// </remarks>
        public IEnumerable<string> GetRawMetadataValues(string name) => Items.Select(item => item.GetMetadata(name)?.UnevaluatedValue);

        /// <summary>
        ///     Get the evaluated value of the specified metadata for the first item in the group.
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
        public string GetFirstMetadataValue(string name) => FirstItem.GetMetadataValue(name);

        /// <summary>
        ///     Get the raw (unevaluated) value of the specified metadata for the first item in the group.
        /// </summary>
        /// <param name="name">
        ///     The metadata name.
        /// </param>
        /// <returns>
        ///     The metadata's unevaluated value, or <c>null</c> if no metadata was found with the specified name.
        /// </returns>
        /// <remarks>
        ///     Cannot be used to access built-in metadata (such as "FullPath"); use <see cref="GetFirstMetadataValue"/>, instead.
        /// </remarks>
        public string GetFirstRawMetadataValue(string name) => FirstItem.GetMetadata(name)?.UnevaluatedValue;
    }
}
