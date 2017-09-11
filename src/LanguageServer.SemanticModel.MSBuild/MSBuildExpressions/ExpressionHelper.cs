using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    using System.Collections.Generic;
    using Utilities;

    /// <summary>
    ///     Helper methods for working with <see cref="ExpressionNode"/>s.
    /// </summary>
    static class ExpressionHelper
    {
        /// <summary>
        ///     Perform post-parse processing on the node to ensure that <see cref="ExpressionNode.Range"/>s are populated and children are connected via the usual relationships (<see cref="ExpressionNode.Parent"/>, <see cref="ExpressionNode.PreviousSibling"/>, and <see cref="ExpressionNode.NextSibling"/>).
        /// </summary>
        /// <typeparam name="TNode">
        ///     The root node type.
        /// </typeparam>
        /// <param name="root">
        ///     The root node.
        /// </param>
        /// <param name="textPositions">
        ///     A <see cref="TextPositions"/> used to map absolute node positions to line / column.
        /// </param>
        /// <returns>
        ///     The root node (enables inline use).
        /// </returns>
        public static TNode PostParse<TNode>(this TNode root, TextPositions textPositions)
            where TNode : ExpressionNode
        {
            if (root == null)
                throw new System.ArgumentNullException(nameof(root));

            if (textPositions == null)
                throw new System.ArgumentNullException(nameof(textPositions));

            Dictionary<int, Position> positionCache = new Dictionary<int, Position>();
            void SetRange(ExpressionNode node)
            {
                Position start;
                if (!positionCache.TryGetValue(node.AbsoluteStart, out start))
                {
                    start = textPositions.GetPosition(node.AbsoluteStart);
                    positionCache.Add(node.AbsoluteStart, start);
                }

                Position end;
                if (!positionCache.TryGetValue(node.AbsoluteEnd, out end))
                {
                    end = textPositions.GetPosition(node.AbsoluteEnd);
                    positionCache.Add(node.AbsoluteEnd, end);
                }

                node.Range = new Range(start, end);
            }

            SetRange(root);

            foreach (ExpressionContainerNode parent in root.DescendantNodes().OfType<ExpressionContainerNode>())
            {
                SetRange(root);

                ExpressionNode previousSibling = null;
                foreach (ExpressionNode nextSibling in parent.Children)
                {
                    SetRange(nextSibling);

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
