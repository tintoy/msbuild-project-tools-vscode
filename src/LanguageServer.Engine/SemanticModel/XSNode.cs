using Microsoft.Language.Xml;
using System;

using MLXML = Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents an XML node in the semantic model.
    /// </summary>
    public abstract class XSNode
    {
        // TODO: Consider storing TextPositions here to allow XSNode and friends to calculate Positions and Ranges as-needed.

        /// <summary>
        ///     Create a new <see cref="XSNode"/>.
        /// </summary>
        /// <param name="range">
        ///     The <see cref="Range"/>, within the source text, spanned by the node.
        /// </param>
        /// <param name="parent">
        ///     The node's parent (if any).
        /// </param>
        protected XSNode(Range range, XSNode parent)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));
            
            Range = range;
            Parent = parent;
        }

        /// <summary>
        ///     The <see cref="Range"/>, within the source text, spanned by the node.
        /// </summary>
        public Range Range { get; }

        /// <summary>
        ///     The kind of XML node represented by the <see cref="XSNode"/>.
        /// </summary>
        public abstract XSNodeKind Kind { get; }

        /// <summary>
        ///     Does the <see cref="XSNode"/> represent valid XML?
        /// </summary>
        public abstract bool IsValid { get; }

        /// <summary>
        ///     The node's parent (if any).
        /// </summary>
        public XSNode Parent { get; private set; }

        /// <summary>
        ///     Create a copy of the <see cref="XSNode"/>, but with the specified parent node.
        /// </summary>
        /// <param name="parent">
        ///     The parent node, or <c>null</c> if the new node should have no parent.
        /// </param>
        /// <returns>
        ///     The new node.
        /// </returns>
        protected XSNode WithParent(XSNode parent)
        {
            if (ReferenceEquals(Parent, parent))
                return this;

            XSNode clone = Clone();
            clone.Parent = parent;

            return clone;
        }

        /// <summary>
        ///     Clone the <see cref="XSNode"/>.
        /// </summary>
        /// <returns>
        ///     The clone.
        /// </returns>
        protected abstract XSNode Clone();
    }

    /// <summary>
    ///     Represents an XML node in the semantic model with a known type of corresponding <see cref="MLXML.SyntaxNode"/>.
    /// </summary>
    /// <typeparam name="TSyntax">
    ///     The type of <see cref="MLXML.SyntaxNode"/> represented by the <see cref="XSNode{TSyntax}"/>.
    /// </typeparam>
    public abstract class XSNode<TSyntax>
        : XSNode
        where TSyntax : SyntaxNode
    {   
        /// <summary>
        ///     Create a new <see cref="XSNode{TSyntax}"/>.
        /// </summary>
        /// <param name="syntaxNode">
        ///     The <typeparamref name="TSyntax"/> represented by the <see cref="XSNode{TSyntax}"/>.
        /// </param>
        /// <param name="range">
        ///     The <see cref="Range"/>, within the source text, spanned by the node.
        /// </param>
        /// <param name="parent">
        ///     The node's parent (if any).
        /// </param>
        protected XSNode(TSyntax syntaxNode, Range range, XSNode parent)
            : base(range, parent)
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));
            
            SyntaxNode = syntaxNode;
        }

        /// <summary>
        ///     The underlying <typeparamref name="TSyntax"/> represented by the <see cref="XSNode{TSyntax}"/>.
        /// </summary>
        protected TSyntax SyntaxNode { get; }
    }
}
