using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents a path through XML.
    /// </summary>
    public sealed class XSPath // TODO: IEquatable<XSPath> (plus equality / inequality operators)
    {
        /// <summary>
        ///     The <see cref="XSPath"/> separator.
        /// </summary>
        internal static readonly char PathSeparatorCharacter = '/';

        /// <summary>
        ///     The <see cref="XSPath"/> separator.
        /// </summary>
        static readonly string PathSeparatorString = PathSeparatorCharacter.ToString();

        /// <summary>
        ///     The path of the root <see cref="XSPath"/>.
        /// </summary>
        static readonly string RootPath = PathSeparatorString;

        /// <summary>
        ///     The root path.
        /// </summary>
        public static readonly XSPath Root = new XSPath(
            ancestorSegments: ImmutableList<XSPathSegment>.Empty,
            segments: ImmutableList<XSPathSegment>.Empty.Add(XSPathSegment.Root)
        );

        /// <summary>
        ///     The path's ancestor segments.
        /// </summary>
        /// <remarks>
        ///     We store these separately because not every <see cref="XSPath"/> is created with a parent <see cref="XSPath"/>.
        /// </remarks>
        readonly ImmutableList<XSPathSegment> _ancestorSegments;

        /// <summary>
        ///     The path's segments, including the leaf.
        /// </summary>
        readonly ImmutableList<XSPathSegment> _segments;

        /// <summary>
        ///     The parent path (if any).
        /// </summary>
        readonly XSPath _parent;

        /// <summary>
        ///     The path as a string (lazily-computed).
        /// </summary>
        string _path;

        /// <summary>
        ///     Create a new <see cref="XSPath"/>.
        /// </summary>
        /// <param name="ancestorSegments">
        ///     The path's ancestor segments.
        /// </param>
        /// <param name="segments">
        ///     The path's segments, including the leaf.
        /// </param>
        XSPath(ImmutableList<XSPathSegment> ancestorSegments, ImmutableList<XSPathSegment> segments)
        {
            if (ancestorSegments == null)
                throw new ArgumentNullException(nameof(ancestorSegments));

            if (segments == null)
                throw new ArgumentNullException(nameof(segments));

            _ancestorSegments = ancestorSegments;
            _segments = segments;
        }

        /// <summary>
        ///     Create a new <see cref="XSPath"/> by appending a child path to a parent path.
        /// </summary>
        /// <param name="parent">
        ///     The parent <see cref="XSPath"/>.
        /// </param>
        /// <param name="child">
        ///     The child <see cref="XSPath"/>.
        /// </param>
        XSPath(XSPath parent, XSPath child)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (child == null)
                throw new ArgumentNullException(nameof(child));

            _parent = parent;
            _ancestorSegments = parent._segments.AddRange(child._ancestorSegments);
            _segments = _ancestorSegments.Add(child.Leaf);
        }

        /// <summary>
        ///     Create a new <see cref="XSPath"/> by adding a leaf <see cref="XSPathSegment"/> onto the parent <see cref="XSPath"/>'s list of segments.
        /// </summary>
        /// <param name="parent">
        ///     The parent <see cref="XSPath"/>.
        /// </param>
        /// <param name="leaf">
        ///     The leaf <see cref="XSPathSegment"/> to append.
        /// </param>
        XSPath(XSPath parent, XSPathSegment leaf)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (leaf == null)
                throw new ArgumentNullException(nameof(leaf));

            _parent = parent;
            _ancestorSegments = _parent._segments; ;
            _segments = _ancestorSegments.Add(leaf);
        }

        /// <summary>
        ///     The path's string representation.
        /// </summary>
        public string Path => _path ?? (_path = ComputePathString());

        /// <summary>
        ///     The name of the current path segment.
        /// </summary>
        public string Name => Leaf.Name;

        /// <summary>
        ///     The path's ancestor segments.
        /// </summary>
        public ImmutableList<XSPathSegment> Ancestors => _ancestorSegments;

        /// <summary>
        ///     The path's segments, including the leaf.
        /// </summary>
        public ImmutableList<XSPathSegment> Segments => _segments;

        /// <summary>
        ///     The last segment of the path.
        /// </summary>
        public XSPathSegment Leaf => _segments[_segments.Count - 1];

        /// <summary>
        ///     Is the path an absolute path?
        /// </summary>
        public bool IsAbsolute => _segments[0] == XSPathSegment.Root;

        /// <summary>
        ///     Is the path a relative path?
        /// </summary>
        public bool IsRelative => !IsAbsolute;

        /// <summary>
        ///     An <see cref="XSPath"/> representing the path's parent (i.e. without the leaf node).
        /// </summary>
        public XSPath Parent
        {
            get
            {
                // If we were constructed with a parent, just use that.
                if (_parent != null)
                    return _parent;

                // Logical short-circuit; no ancestors means no parent.
                if (_ancestorSegments.Count == 0)
                    return null;

                // We weren't constructed with a parent, so we'll need to create one (this is less efficient but unavoidable in some cases).

                return new XSPath(
                    ancestorSegments: _ancestorSegments.RemoveAt(_ancestorSegments.Count - 1),
                    segments: _ancestorSegments
                );
            }
        }

        /// <summary>
        ///     Determine whether the <see cref="XSPath"/> starts the specified base path.
        /// </summary>
        /// <param name="basePath">
        ///     The other <see cref="XSPath"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the <see cref="XSPath"/> starts with the same segments as the other <see cref="XSPath"/>.
        /// </returns>
        public bool StartsWith(XSPath basePath)
        {
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            return _segments.StartsWith(basePath._segments);
        }

        /// <summary>
        ///     Determine whether the <see cref="XSPath"/> has ends with the the specified path.
        /// </summary>
        /// <param name="ancestorPath">
        ///     The other <see cref="XSPath"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the other <see cref="XSPath"/>'s trailing segments are the same as the <see cref="XSPath"/>'s segments.
        /// </returns>
        public bool EndsWith(XSPath ancestorPath)
        {
            if (ancestorPath == null)
                throw new ArgumentNullException(nameof(ancestorPath));

            if (ancestorPath.IsAbsolute)
                return StartsWith(ancestorPath);
            
            if (_ancestorSegments.Count == 0 && Leaf == ancestorPath.Leaf)
                return true;

            return _segments.EndsWith(ancestorPath._segments);
        }

        /// <summary>
        ///     Determine whether the <see cref="XSPath"/> has the specified path as its direct ancestor.
        /// </summary>
        /// <param name="parentPath">
        ///     The other <see cref="XSPath"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the other <see cref="XSPath"/>'s trailing segments are the same as the <see cref="XSPath"/>'s ancestor segments.
        /// </returns>
        public bool IsChildOf(XSPath parentPath)
        {
            if (parentPath == null)
                throw new ArgumentNullException(nameof(parentPath));

            if (IsAbsolute && !parentPath.IsAbsolute)
                return false; // Logical short-circuit: absolute path cannot be a child of another path.

            // Special case: any relative path is considered a child of the root.
            if (IsRelative && parentPath == Root)
                return true;

            return _ancestorSegments.EndsWith(parentPath._segments);
        }

        /// <summary>
        ///     Determine whether the <see cref="XSPath"/> has the specified path as its direct child.
        /// </summary>
        /// <param name="childPath">
        ///     The other <see cref="XSPath"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the other <see cref="XSPath"/>'s ancestor segments are the same as the <see cref="XSPath"/>'s trailing segments.
        /// </returns>
        public bool IsParentOf(XSPath childPath)
        {
            if (childPath == null)
                throw new ArgumentNullException(nameof(childPath));

            return childPath.IsChildOf(this);
        }

        /// <summary>
        ///     Append an <see cref="XSPath"/> to the <see cref="XSPath"/>.
        /// </summary>
        /// <param name="path">
        ///     The <see cref="XSPath"/> to append.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public XSPath Append(XSPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.IsAbsolute)
                return path;

            return new XSPath(
                parent: this,
                child: path
            );
        }

        /// <summary>
        ///     Append <see cref="XSPathSegment"/>s to the <see cref="XSPath"/>.
        /// </summary>
        /// <param name="pathSegments">
        ///     The <see cref="XSPathSegment"/>s to append.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public XSPath Append(IEnumerable<XSPathSegment> pathSegments)
        {
            if (pathSegments == null)
                throw new ArgumentNullException(nameof(pathSegments));

            XSPathSegment[] actualPathSegments = pathSegments.ToArray();
            if (actualPathSegments.Length == 0)
                return this;

            ImmutableList<XSPathSegment> ancestorSegments = ImmutableList.CreateRange(
                actualPathSegments.Take(actualPathSegments.Length - 1)
            );
            ImmutableList<XSPathSegment> segments = ancestorSegments.Add(
                actualPathSegments[actualPathSegments.Length - 1]
            );

            XSPath appendPath = new XSPath(ancestorSegments, segments);
            if (appendPath.IsAbsolute)
                return appendPath;
            
            return new XSPath(
                parent: this,
                child: appendPath
            );
        }

        /// <summary>
        ///     Append a segment to the path.
        /// </summary>
        /// <param name="pathSegment">
        ///     The segment to append.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public XSPath Append(XSPathSegment pathSegment)
        {
            if (pathSegment == null)
                throw new ArgumentNullException(nameof(pathSegment));

            return new XSPath(
                parent: this,
                leaf: pathSegment
            );
        }

        /// <summary>
        ///     Append a path or segment to the path.
        /// </summary>
        /// <param name="pathOrSegment">
        ///     The path or path-segment to append.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public XSPath Append(string pathOrSegment)
        {
            if (pathOrSegment == null)
                throw new ArgumentNullException(nameof(pathOrSegment));

            if (pathOrSegment.IndexOf(PathSeparatorCharacter) != -1)
            {
                return Append(
                    Parse(pathOrSegment)
                );
            }

            return Append(
                XSPathSegment.Create(pathOrSegment)
            );
        }

        /// <summary>
        ///     Create an <see cref="XSPath"/> containing a single path segment.
        /// </summary>
        /// <param name="pathSegment">
        ///     The path segment.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public static XSPath FromSegment(string pathSegment)
        {
            if (pathSegment == null)
                throw new ArgumentNullException(nameof(pathSegment));

            return FromSegment(
                XSPathSegment.Create(pathSegment)
            );
        }

        /// <summary>
        ///     Create an <see cref="XSPath"/> containing a single path segment.
        /// </summary>
        /// <param name="pathSegment">
        ///     The path segment.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public static XSPath FromSegment(XSPathSegment pathSegment)
        {
            if (pathSegment == null)
                throw new ArgumentNullException(nameof(pathSegment));

            if (pathSegment == XSPathSegment.Root)
                return Root;

            return new XSPath(
                ImmutableList<XSPathSegment>.Empty,
                ImmutableList<XSPathSegment>.Empty.Add(pathSegment)
            );
        }

        /// <summary>
        ///     Get a string representation of the <see cref="XSPath"/>.
        /// </summary>
        /// <returns>
        ///     The path segments, separated by <see cref="PathSeparatorCharacter"/>s.
        /// </returns>
        public override string ToString() => Path;

        /// <summary>
        ///     Compute the path's string representation.
        /// </summary>
        /// <returns>
        ///     The path's string representation.
        /// </returns>
        string ComputePathString()
        {
            if (_segments.Count == 1 && _segments[0] == XSPathSegment.Root)
                return RootPath;

            return String.Join(PathSeparatorString,
                _segments.Select(segment => segment.Name)
            );
        }

        /// <summary>
        ///     Parse a string into an <see cref="XSPath"/>.
        /// </summary>
        /// <param name="path">
        ///     The string to parse.
        /// </param>
        /// <returns>
        ///     The <see cref="XSPath"/>.
        /// </returns>
        public static XSPath Parse(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path == PathSeparatorString)
                return Root;

            if (path[path.Length - 1] == PathSeparatorCharacter)
                path = path.Substring(0, path.Length - 1);

            XSPathSegment[] pathSegments = ParseSegments(path).ToArray();

            ImmutableList<XSPathSegment> ancestorSegments = ImmutableList.CreateRange(
                pathSegments.Take(pathSegments.Length - 1)
            );
            ImmutableList<XSPathSegment> segments = ancestorSegments.Add(
                pathSegments[pathSegments.Length - 1]
            );

            return new XSPath(ancestorSegments, segments);
        }

        /// <summary>
        ///     Parse a string into an <see cref="XSPathSegment"/>s.
        /// </summary>
        /// <param name="path">
        ///     The string to parse.
        /// </param>
        /// <returns>
        ///     The <see cref="XSPath"/>.
        /// </returns>
        public static IEnumerable<XSPathSegment> ParseSegments(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path == PathSeparatorString)
                yield return XSPathSegment.Root;

            if (path[path.Length - 1] == PathSeparatorCharacter)
                path = path.Substring(0, path.Length - 1);

            string[] pathSegments = path.Split(new char[] { PathSeparatorCharacter });
            foreach (string pathSegment in pathSegments)
                yield return XSPathSegment.Create(pathSegment);
        }

        /// <summary>
        ///     Append an <see cref="XSPathSegment"/> to an <see cref="XSPath"/>.
        /// </summary>
        /// <param name="path">
        ///     The <see cref="XSPath"/>.
        /// </param>
        /// <param name="segment">
        ///     The <see cref="XSPathSegment"/> to append.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public static XSPath operator +(XSPath path, XSPathSegment segment)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (segment == null)
                throw new ArgumentNullException(nameof(segment));

            return path.Append(segment);
        }

        /// <summary>
        ///     Append a string (path segment) to an <see cref="XSPath"/>.
        /// </summary>
        /// <param name="path">
        ///     The <see cref="XSPath"/>.
        /// </param>
        /// <param name="pathOrSegment">
        ///     The string (path or path-segment) to append.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public static XSPath operator +(XSPath path, string pathOrSegment)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (pathOrSegment == null)
                throw new ArgumentNullException(nameof(pathOrSegment));

            return path.Append(pathOrSegment);
        }
    }
}
