using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Represents a path through XML.
    /// </summary>
    public sealed class XSPath
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
        readonly ImmutableList<XSPathSegment> _ancestorSegments;

        /// <summary>
        ///     The path's segments, including the leaf.
        /// </summary>
        readonly ImmutableList<XSPathSegment> _segments;

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
                if (_ancestorSegments.Count == 0)
                    return null;

                return new XSPath(
                    ancestorSegments: _ancestorSegments.RemoveAt(_ancestorSegments.Count - 1),
                    segments: _ancestorSegments
                );
            }
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

            ImmutableList<XSPathSegment> ancestorSegments = _segments.AddRange(path._ancestorSegments);
            ImmutableList<XSPathSegment> segments = ancestorSegments.Add(path.Leaf);

            return new XSPath(ancestorSegments, segments);
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

            return new XSPath(_segments,
                _segments.Add(pathSegment)
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

            string[] pathSegments = path.Split(new char[] { PathSeparatorCharacter });

            ImmutableList<XSPathSegment> ancestorSegments = ImmutableList.CreateRange(
                pathSegments.Take(pathSegments.Length - 1).Select(XSPathSegment.Create)
            );
            ImmutableList<XSPathSegment> segments = ancestorSegments.Add(
                XSPathSegment.Create(
                    pathSegments[pathSegments.Length - 1]
                )
            );

            return new XSPath(ancestorSegments, segments);
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
        /// <param name="segment">
        ///     The string (path segment) to append.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public static XSPath operator +(XSPath path, string segment)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (segment == null)
                throw new ArgumentNullException(nameof(segment));

            return path.Append(segment);
        }
    }

    /// <summary>
    ///     A single segment of an <see cref="XSPath"/>.
    /// </summary>
    public sealed class XSPathSegment
        : IEquatable<XSPathSegment>
    {
        /// <summary>
        ///     The root path segment.
        /// </summary>
        public static readonly XSPathSegment Root = new XSPathSegment(String.Empty);

        /// <summary>
        ///     Create a new <see cref="XSPathSegment"/>.
        /// </summary>
        /// <param name="name">
        ///     The path segment's name.
        /// </param>
        XSPathSegment(string name)
        {
            if (string.IsNullOrWhiteSpace(name) && name != String.Empty)
                throw new ArgumentException($"Argument cannot be null or entirely composed of whitespace: {nameof(name)}.", nameof(name));

            if (name.IndexOf(XSPath.PathSeparatorCharacter) != -1)
                throw new FormatException($"Path segments cannot contain the path separator character ('{XSPath.PathSeparatorCharacter}').");

            Name = name;
        }

        /// <summary>
        ///     The path segment's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Determine whether the <see cref="XSPathSegment"/> is equal to another <see cref="XSPathSegment"/>.
        /// </summary>
        /// <param name="other">
        ///     The other <see cref="XSPathSegment"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the <see cref="XSPathSegment"/> is equal to the other <see cref="XSPathSegment"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(XSPathSegment other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return other.Name == Name;
        }

        /// <summary>
        ///     Determine whether the <see cref="XSPathSegment"/> is equal to another <see cref="Object"/>.
        /// </summary>
        /// <param name="other">
        ///     The other <see cref="Object"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the <see cref="XSPathSegment"/> is equal to the other <see cref="Object"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            if (other is XSPathSegment otherPathSegment)
                return Equals(otherPathSegment);

            return base.Equals(other);
        }

        /// <summary>
        ///     Get a hash code representing the <see cref="XSPathSegment"/>.
        /// </summary>
        /// <returns>
        ///     The hash code.
        /// </returns>
        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        ///     Get a string representation of the path segment.
        /// </summary>
        /// <returns>
        ///     The path segment's name.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        ///     Create a new <see cref="XSPathSegment"/>.
        /// </summary>
        /// <param name="name">
        ///     The path segment's name.
        /// </param>
        /// <returns>
        ///     The <see cref="XSPathSegment"/>.
        /// </returns>
        public static XSPathSegment Create(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name == String.Empty)
                return Root;

            return new XSPathSegment(name);
        }

        /// <summary>
        ///     Determine whether 2 <see cref="XSPathSegment"/>s are equal.
        /// </summary>
        /// <param name="left">
        ///     The left-hand <see cref="XSPathSegment"/>.
        /// </param>
        /// <param name="right">
        ///     The right-hand <see cref="XSPathSegment"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the <see cref="XSPathSegment"/>s are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(XSPathSegment left, XSPathSegment right)
        {
            bool isLeftNull = ReferenceEquals(left, null);
            bool isRightNull = ReferenceEquals(right, null);

            if (isLeftNull && isRightNull)
                return true;

            if (isLeftNull || isRightNull)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        ///     Determine whether 2 <see cref="XSPathSegment"/>s are not equal.
        /// </summary>
        /// <param name="left">
        ///     The left-hand <see cref="XSPathSegment"/>.
        /// </param>
        /// <param name="right">
        ///     The right-hand <see cref="XSPathSegment"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c>, if the <see cref="XSPathSegment"/>s are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(XSPathSegment left, XSPathSegment right)
        {
            bool isLeftNull = ReferenceEquals(left, null);
            bool isRightNull = ReferenceEquals(right, null);

            if (isLeftNull && isRightNull)
                return false;

            if (isLeftNull || isRightNull)
                return true;

            return !left.Equals(right);
        }

        /// <summary>
        ///     Concatenate 2 <see cref="XSPathSegment"/>s to create an <see cref="XSPath"/>.
        /// </summary>
        /// <param name="left">
        ///     The left-hand <see cref="XSPathSegment"/>.
        /// </param>
        /// <param name="right">
        ///     The right-hand <see cref="XSPathSegment"/>.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public static XSPath operator +(XSPathSegment left, XSPathSegment right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            return XSPath.FromSegment(left) + right;
        }

        /// <summary>
        ///     Concatenate an <see cref="XSPathSegment"/> and a string to create an <see cref="XSPath"/>.
        /// </summary>
        /// <param name="left">
        ///     The left-hand <see cref="XSPathSegment"/>.
        /// </param>
        /// <param name="right">
        ///     The right-hand path segment.
        /// </param>
        /// <returns>
        ///     The new <see cref="XSPath"/>.
        /// </returns>
        public static XSPath operator +(XSPathSegment left, string right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            return XSPath.FromSegment(left) + right;
        }
    }
}
