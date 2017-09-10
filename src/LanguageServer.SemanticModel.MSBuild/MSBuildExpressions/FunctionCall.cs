using Sprache;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Represents an MSBuild function-call expression.
    /// </summary>
    public class FunctionCall
        : ExpressionContainerNode, IPositionAware<FunctionCall>
    {
        /// <summary>
        ///     Create a new <see cref="FunctionCall"/>.
        /// </summary>
        public FunctionCall()
        {
        }

        /// <summary>
        ///     The node kind.
        /// </summary>
        public override ExpressionKind Kind => ExpressionKind.FunctionCall;

        /// <summary>
        ///     The function name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        ///     The type of function represented by the function-call expression.
        /// </summary>
        public FunctionKind FunctionKind { get; internal set; }

        /// <summary>
        ///     The target of the function-call (<c>null</c>, for <see cref="FunctionKind.Global"/> functions).
        /// </summary>
        public ExpressionNode Target => FunctionKind != FunctionKind.Global ? Children[0] : null;

        /// <summary>
        ///     The function-call's arguments (if any).
        /// </summary>
        public IEnumerable<ExpressionNode> Arguments => FunctionKind != FunctionKind.Global ? Children.Skip(1) : Children;

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
        FunctionCall IPositionAware<FunctionCall>.SetPos(Sprache.Position startPosition, int length)
        {
            SetPosition(startPosition, length);

            return this;
        }
    }
}
