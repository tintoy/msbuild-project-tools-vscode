using System;
using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild item metadata expression.
    /// </summary>
    public class ItemMetadata
        : ExpressionContainerNode, IPositionAware<ItemMetadata>
    {
        /// <summary>
        ///     Create a new <see cref="ItemMetadata"/> expression.
        /// </summary>
        public ItemMetadata()
        {
        }

        /// <summary>
        ///     The name of the item metadata.
        /// </summary>
        public string Name
        {
            get
            {
                if (HasItemType)
                    return GetChild<Symbol>(1).Name;

                if (HasName)
                    return GetChild<Symbol>(0).Name;

                return String.Empty;
            }
        }

        /// <summary>
        ///     The name of the item type on which the metadata is declared.
        /// </summary>
        /// <remarks>
        ///     May be empty for raw metadata expressions (e.g. "%(FullPath)" as opposed to "%(MyItem.FullPath)").
        /// </remarks>
        public string ItemType
        {
            get
            {
                if (HasItemType)
                    return GetChild<Symbol>(0).Name;

                return String.Empty;
            }
        }

        /// <summary>
        ///     Does the metadata expression specify an item type?
        /// </summary>
        public bool HasName => Children.Count > 0 && Children[0] is Symbol;

        /// <summary>
        ///     Does the metadata expression specify an item type?
        /// </summary>
        public bool HasItemType => Children.Count > 1 && Children[0] is Symbol;

        /// <summary>
        ///     Is the item metadata expression valid?
        /// </summary>
        public override bool IsValid => !String.IsNullOrWhiteSpace(Name) && base.IsValid;

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.ItemMetadata;

        /// <summary>
        ///     Update positioning information.
        /// </summary>
        /// <param name="startPosition">
        ///     The node's starting position.
        /// </param>
        /// <param name="length">
        ///     The node length.
        /// </param>
        /// <returns>
        ///     The <see cref="ExpressionNode"/>.
        /// </returns>
        ItemMetadata IPositionAware<ItemMetadata>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
