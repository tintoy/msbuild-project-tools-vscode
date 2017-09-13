using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    using MSBuildExpressions;
    using System.Linq;

    /// <summary>
    ///     Extension methods for <see cref="ExpressionNode"/>.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     Enumerate all of the node's ancestor nodes, up to the root node.
        /// </summary>
        /// <param name="node">
        ///     The target node.
        /// </param>
        /// <returns>
        ///     A sequence of ancestor nodes.
        /// </returns>
        public static IEnumerable<ExpressionNode> AncestorNodes(this ExpressionNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            ExpressionNode parent = node.Parent;
            while (parent != null)
            {
                yield return parent;

                parent = parent.Parent;
            }
        }

        /// <summary>
        ///     Recursively enumerate the node's descendant nodes.
        /// </summary>
        /// <param name="node">
        ///     The target node.
        /// </param>
        /// <returns>
        ///     A sequence of descendant nodes (depth-first).
        /// </returns>
        public static IEnumerable<ExpressionNode> DescendantNodes(this ExpressionNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            ExpressionContainerNode containerNode = node as ExpressionContainerNode;
            if (containerNode == null)
                yield break;

            foreach (ExpressionNode childNode in containerNode.Children)
            {
                yield return node;

                foreach (ExpressionNode descendant in childNode.DescendantNodes())
                    yield return descendant;
            }
        }

        /// <summary>
        ///     Find the least-deeply-nested <see cref="ExpressionNode"/> at the specified position.
        /// </summary>
        /// <param name="nodes">
        ///     The root node at which the search is started.
        /// </param>
        /// <param name="atPosition">
        ///     The target position (0-based offset from start of source text).
        /// </param>
        /// <returns>
        ///     The <see cref="ExpressionNode"/> or <c>null</c> if no node was found at the specified position.
        /// </returns>
        public static ExpressionNode FindNodeAt(this IEnumerable<ExpressionNode> nodes, int atPosition)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            return nodes.FirstOrDefault(node =>
            {
                // Special case for "virtual" nodes (they take up no space, but we want to match them when searching).
                if (node.IsVirtual)
                    return atPosition == node.AbsoluteStart;    
                
                return node.AbsoluteLength > 0 && atPosition >= node.AbsoluteStart && atPosition < node.AbsoluteEnd;
            });
        }

        /// <summary>
        ///     Find the least-deeply-nested <see cref="ExpressionNode"/> at the specified position.
        /// </summary>
        /// <param name="nodes">
        ///     The root node at which the search is started.
        /// </param>
        /// <param name="atPosition">
        ///     The target position.
        /// </param>
        /// <returns>
        ///     The <see cref="ExpressionNode"/> or <c>null</c> if no node was found at the specified position.
        /// </returns>
        public static ExpressionNode FindNodeAt(this IEnumerable<ExpressionNode> nodes, Position atPosition)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            return nodes.FirstOrDefault(
                node => atPosition >= node.Start && atPosition < node.End
            );
        }

        /// <summary>
        ///     Find the most-deeply-nested <see cref="ExpressionNode"/> at the specified position.
        /// </summary>
        /// <param name="node">
        ///     The root node at which the search is started.
        /// </param>
        /// <param name="atPosition">
        ///     The target position (0-based offset from start of source text).
        /// </param>
        /// <returns>
        ///     The <see cref="ExpressionNode"/> or <c>null</c> if no node was found at the specified position.
        /// </returns>
        public static ExpressionNode FindDeepestNodeAt(this ExpressionNode node, Position atPosition)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (atPosition < node.Start || atPosition > node.End)
                return null;

            if (node is ExpressionContainerNode containerNode)
            {
                ExpressionNode nodeAtPosition = containerNode.Children.FindNodeAt(atPosition);
                if (nodeAtPosition != null)
                {
                    ExpressionNode deeperNodeAtPosition = nodeAtPosition.FindDeepestNodeAt(atPosition);
                    if (deeperNodeAtPosition != null)
                        return deeperNodeAtPosition;
                }

                return node;
            }
            else
                return node;
        }

        /// <summary>
        ///     Find the most-deeply-nested <see cref="ExpressionNode"/> at the specified position.
        /// </summary>
        /// <param name="node">
        ///     The root node at which the search is started.
        /// </param>
        /// <param name="atPosition">
        ///     The target position (0-based offset from start of source text).
        /// </param>
        /// <returns>
        ///     The <see cref="ExpressionNode"/> or <c>null</c> if no node was found at the specified position.
        /// </returns>
        public static ExpressionNode FindDeepestNodeAt(this ExpressionNode node, int atPosition)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (atPosition < node.AbsoluteStart || atPosition > node.AbsoluteEnd)
                return null;

            if (node is ExpressionContainerNode containerNode)
            {
                ExpressionNode nodeAtPosition = containerNode.Children.FindNodeAt(atPosition);
                if (nodeAtPosition != null)
                {
                    ExpressionNode deeperNodeAtPosition = nodeAtPosition.FindDeepestNodeAt(atPosition);
                    if (deeperNodeAtPosition != null)
                        return deeperNodeAtPosition;
                }

                return node;
            }
            else
                return node;
        }

        /// <summary>
        ///     Find the list item at (or close to) the specified absolute position within the source text.
        /// </summary>
        /// <param name="list">
        ///     The <see cref="SimpleList"/> to search.
        /// </param>
        /// <param name="atPosition">
        ///     The absolute position (0-based).
        /// </param>
        /// <returns>
        ///     The <see cref="SimpleListItem"/>, or <c>null</c> if there is no item at the specified absolute position.
        /// </returns>
        public static SimpleListItem FindItemAt(this SimpleList list, int atPosition)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (atPosition < list.AbsoluteStart || atPosition > list.AbsoluteEnd)
                return null;

            ExpressionNode nodeAtPosition = list.Children.FindLast(
                node => node.AbsoluteStart <= atPosition
            );
            if (nodeAtPosition is SimpleListItem itemAtPosition)
                return itemAtPosition;

            if (nodeAtPosition is ListSeparator separatorAtPosition)
            {
                // If the position is on or before a separator then choose the preceding item; otherwise, choose the next item.
                int separatorPosition = separatorAtPosition.AbsoluteStart + separatorAtPosition.SeparatorOffset;

                return (atPosition <= separatorPosition)
                    ? separatorAtPosition.PreviousSibling as SimpleListItem
                    : separatorAtPosition.NextSibling as SimpleListItem;
            }

            throw new InvalidOperationException(
                $"Encountered unexpected node type '{nodeAtPosition.GetType().FullName}' inside a SimpleList expression."
            );
        }

        /// <summary>
        ///     Filter the expressions to exclude virtual expression nodes.
        /// </summary>
        /// <typeparam name="TExpression">
        ///     The expression sequence type.
        /// </typeparam>
        /// <param name="expressions">
        ///     The sequence of expressions to filter.
        /// </param>
        /// <returns>
        ///     The filtered sequence of expressions.
        /// </returns>
        public static IEnumerable<TExpression> NonVirtual<TExpression>(this IEnumerable<TExpression> expressions)
            where TExpression : ExpressionNode
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            return expressions.Where(expression => !expression.IsVirtual);
        }

        /// <summary>
        ///     Filter the expressions to exclude invalid expressions.
        /// </summary>
        /// <typeparam name="TExpression">
        ///     The expression sequence type.
        /// </typeparam>
        /// <param name="expressions">
        ///     The sequence of expressions to filter.
        /// </param>
        /// <returns>
        ///     The filtered sequence of expressions.
        /// </returns>
        public static IEnumerable<TExpression> ValidOnly<TExpression>(this IEnumerable<TExpression> expressions)
            where TExpression : ExpressionNode
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));

            return expressions.Where(expression => expression.IsValid);
        }
    }
}
