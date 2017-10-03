using System;
using System.Collections.Immutable;

namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     Extension methods for <see cref="XSPathSegment"/>.
    /// </summary>
    internal static class XSPathSegmentExtensions
    {
        /// <summary>
        ///     Determine whether the list of <see cref="XSPathSegment"/>s starts with another list of <see cref="XSPathSegment"/>s.
        /// </summary>
        /// <param name="segments">
        ///     The list of <see cref="XSPathSegment"/>s.
        /// </param>
        /// <param name="otherSegments">
        ///     The other list of <see cref="XSPathSegment"/>s.s
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="segments"/> starts with <paramref name="segments"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool StartsWith(this ImmutableList<XSPathSegment> segments, ImmutableList<XSPathSegment> otherSegments)
        {
            if (segments == null)
                throw new ArgumentNullException(nameof(segments));

            if (otherSegments == null)
                throw new ArgumentNullException(nameof(otherSegments));

            if (segments.Count == 0)
                return false; // Logical short-circuit: can't have a prefix.

            if (otherSegments.Count > segments.Count)
                return false; // Logical short-circuit: can't be a prefix.

            for (int index = 0; index < otherSegments.Count; index++)
            {
                if (index >= segments.Count)
                    return false;

                if (otherSegments[index] != segments[index])
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Determine whether the list of <see cref="XSPathSegment"/>s ends with another list of <see cref="XSPathSegment"/>s.
        /// </summary>
        /// <param name="segments">
        ///     The list of <see cref="XSPathSegment"/>s.
        /// </param>
        /// <param name="otherSegments">
        ///     The other list of <see cref="XSPathSegment"/>s.s
        /// </param>
        /// <returns>
        ///     <c>true</c>, if <paramref name="segments"/> ends with <paramref name="segments"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool EndsWith(this ImmutableList<XSPathSegment> segments, ImmutableList<XSPathSegment> otherSegments)
        {
            if (segments == null)
                throw new ArgumentNullException(nameof(segments));

            if (otherSegments == null)
                throw new ArgumentNullException(nameof(otherSegments));

            if (segments.Count == 0)
                return false; // Logical short-circuit: can't have a prefix.

            if (otherSegments.Count < segments.Count)
                return false; // Logical short-circuit: can't be a prefix.

            int index = segments.Count - 1;
            int ancestorIndex = otherSegments.Count - 1;
            for ( ; index >= 0 && ancestorIndex >= 0; index--, ancestorIndex--)
            {
                if (segments[index] != otherSegments[ancestorIndex])
                    return false;
            }

            return true;
        }
    }
}
