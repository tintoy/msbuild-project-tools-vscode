using System;
using Sprache;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild item group expression.
    /// </summary>
    public class ItemGroup
        : ExpressionContainerNode, IPositionAware<ItemGroup>
    {
        /// <summary>
        ///     Create a new <see cref="ItemGroup"/>.
        /// </summary>
        public ItemGroup()
        {
        }

        /// <summary>
        ///     The item group name.
        /// </summary>
        public string Name => GetChild<Symbol>(0).Name;

        /// <summary>
        ///     Is the item group expression valid?
        /// </summary>
        public override bool IsValid => !String.IsNullOrWhiteSpace(Name) && base.IsValid;

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
        ItemGroup IPositionAware<ItemGroup>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
