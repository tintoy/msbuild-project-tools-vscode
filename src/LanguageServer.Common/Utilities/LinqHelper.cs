using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Helper methods for working with LINQ.
    /// </summary>
    public static class LinqHelper
    {
        /// <summary>
        ///     Flatten the sequence, enumerating nested sequences.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The source element type.
        /// </typeparam>
        /// <param name="source">
        ///     The source sequence of sequences.
        /// </param>
        /// <returns>
        ///     The flattened sequence.
        /// </returns>
        public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<IEnumerable<TSource>> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.SelectMany(items => items);
        }
    }
}
