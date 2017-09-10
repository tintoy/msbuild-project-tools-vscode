using Sprache;
using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild comparison expression.
    /// </summary>
    public class Symbol
        : ExpressionNode, IPositionAware<Symbol>
    {
        /// <summary>
        ///     Create a new <see cref="Symbol"/>.
        /// </summary>
        public Symbol()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.Symbol;

        /// <summary>
        ///     The symbol's name.
        /// </summary>
        public string Name { get; internal set; } = String.Empty;

        /// <summary>
        ///     The symbol's namespace.
        /// </summary>
        public string Namespace { get; set; } = String.Empty;

        /// <summary>
        ///     The symbol's fully-qualified name.
        /// </summary>
        public string FullName => IsQualified ? String.Format("{0}.{1}", Namespace, Name) : Name;

        /// <summary>
        ///     Is the symbol qualified (i.e. does it have a namespace)?
        /// </summary>
        public bool IsQualified => !String.IsNullOrWhiteSpace(Namespace);

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
        Symbol IPositionAware<Symbol>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
