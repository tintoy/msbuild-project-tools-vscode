using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild quoted-string expression.
    /// </summary>
    public class QuotedString
        : ExpressionContainerNode, IPositionAware<QuotedString>
    {
        /// <summary>
        ///     Create a new <see cref="QuotedString"/>.
        /// </summary>
        public QuotedString()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.QuotedString;

        /// <summary>
        ///     Evaluation expressions (if any) contained in the string.
        /// </summary>
        public IEnumerable<Evaluate> Evaluations => Children.OfType<Evaluate>();

        /// <summary>
        ///     The quoted string's textual content (without evaluation expressions).
        /// </summary>
        public virtual string StringContent => String.Join("",
            Children.OfType<StringContent>().Select(
                stringContent => stringContent.Content
            )
        );

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
        QuotedString IPositionAware<QuotedString>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
