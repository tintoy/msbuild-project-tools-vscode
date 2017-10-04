using System;
using System.Collections.Generic;
using System.Text;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Extension methods for working with <see cref="XSNode"/>s.
    /// </summary>
    public static class XSNodeExtensions
    {
        /// <summary>
        ///     Determine whether the <see cref="XSNode"/>'s path starts with the specified <see cref="XSPath"/>.
        /// </summary>
        /// <param name="node">
        ///     The <see cref="XSNode"/>.
        /// </param>
        /// <param name="path">
        ///     The <see cref="XSPath"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <see cref="XSNode.Path"/> starts with <paramref name="path"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool PathStartsWith(this XSNode node, XSPath path)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return node.Path.StartsWith(path);
        }

        /// <summary>
        ///     Determine whether the <see cref="XSNode"/>'s path ends with the specified <see cref="XSPath"/>.
        /// </summary>
        /// <param name="node">
        ///     The <see cref="XSNode"/>.
        /// </param>
        /// <param name="path">
        ///     The <see cref="XSPath"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <see cref="XSNode.Path"/> ends with <paramref name="path"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool PathEndsWith(this XSNode node, XSPath path)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return node.Path.EndsWith(path);
        }

        /// <summary>
        ///     Determine whether the <see cref="XSNode"/>'s parent path is equal to the specified <see cref="XSPath"/>.
        /// </summary>
        /// <param name="node">
        ///     The <see cref="XSNode"/>.
        /// </param>
        /// <param name="parentPath">
        ///     The <see cref="XSPath"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the node's <see cref="XSPath.Parent"/> path is equal to <paramref name="parentPath"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasParentPath(this XSNode node, XSPath parentPath)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (parentPath == null)
                throw new ArgumentNullException(nameof(parentPath));

            XSPath nodeParentPath = node.Path.Parent;
            if (nodeParentPath == null)
                return false;

            // The common use case for this is checking if an element or attribute matches a relative parent path (e.g. match both Project/ItemGroup and Project/Target/ItemGroup).
            if (parentPath.IsRelative)
                return nodeParentPath.EndsWith(parentPath);

            return node.Path.IsChildOf(parentPath);
        }
    }
}
