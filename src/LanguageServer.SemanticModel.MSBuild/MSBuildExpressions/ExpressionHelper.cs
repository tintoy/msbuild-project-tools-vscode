using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Helper methods for working with <see cref="ExpressionNode"/>s.
    /// </summary>
    static class ExpressionHelper
    {
        /// <summary>
        ///     Ensure that the node's children are connected via the usual relationships (<see cref="ExpressionNode.Parent"/>, <see cref="ExpressionNode.PreviousSibling"/>, and <see cref="ExpressionNode.NextSibling"/>).
        /// </summary>
        /// <typeparam name="TNode">
        ///     The root node type.
        /// </typeparam>
        /// <param name="root">
        ///     The root node.
        /// </param>
        /// <returns>
        ///     The root node (enables inline use).
        /// </returns>
        public static TNode EnsureRelationships<TNode>(this TNode root)
            where TNode : ExpressionNode
        {
            foreach (ExpressionContainerNode parent in root.DescendantNodes().OfType<ExpressionContainerNode>())
            {
                ExpressionNode previousSibling = null;
                foreach (ExpressionNode nextSibling in parent.Children)
                {
                    nextSibling.Parent = parent;
                    nextSibling.PreviousSibling = previousSibling;
                    if (previousSibling != null)
                        previousSibling.NextSibling = nextSibling;

                    previousSibling = nextSibling;
                }
            }

            return root;
        }
    }
}
