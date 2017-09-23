using System;
using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild item group expression.
    /// </summary>
    public class ItemGroupTransform
        : ExpressionContainerNode, IPositionAware<ItemGroupTransform>
    {
        /// <summary>
        ///     Create a new <see cref="ItemGroupTransform"/>.
        /// </summary>
        public ItemGroupTransform()
        {
        }

        /// <summary>
        ///     Does te item group expression have a name?
        /// </summary>
        public bool HasName => !String.IsNullOrWhiteSpace(Name);

        /// <summary>
        ///     The item group name.
        /// </summary>
        public string Name => Children.Count > 0 ? GetChild<Symbol>(0).Name : null;

        /// <summary>
        ///     Does the <see cref="ItemGroupTransform"/> expression have a body?
        /// </summary>
        public bool HasBody => Children.Count > 1;

        /// <summary>
        ///     The item group transform's expression body (if any).
        /// </summary>
        public QuotedString Body => HasBody ? GetChild<QuotedString>(1) : null;

        /// <summary>
        ///     Does the <see cref="ItemGroupTransform"/> expression declare a custom separator?
        /// </summary>
        public bool HasSeparator => Children.Count > 2;

        /// <summary>
        ///     The item group transform's custom separator (if any).
        /// </summary>
        public QuotedStringLiteral Separator => HasSeparator ? GetChild<QuotedStringLiteral>(2) : null;

        /// <summary>
        ///     Is the item group transform expression valid?
        /// </summary>
        public override bool IsValid => HasName && HasBody && base.IsValid;

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.ItemGroup;

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
        ItemGroupTransform IPositionAware<ItemGroupTransform>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
