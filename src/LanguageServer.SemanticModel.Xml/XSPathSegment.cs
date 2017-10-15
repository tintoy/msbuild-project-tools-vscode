using System;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
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
        ///     The wild-card ("*") path segment.
        /// </summary>
        /// <remarks>
        ///     A wildcard path segment matches any single path segment.
        /// </remarks>
        public static readonly XSPathSegment Wildcard = new XSPathSegment("*");

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
        ///     Does the path segment represent the root of a path?
        /// </summary>
        public bool IsRoot => Name == String.Empty;

        /// <summary>
        ///     Does the path segment represent a wildcard?
        /// </summary>
        public bool IsWildcard => Name == "*";

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

            if (name == "*")
                return Wildcard;

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
