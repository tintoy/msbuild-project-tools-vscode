using System;
using System.IO;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Extension methods for <see cref="Uri"/>s.
    /// </summary>
    static class UriExtensions
    {
        /// <summary>
        ///     Get the local file-system path for the specified URI.
        /// </summary>
        /// <param name="uri">
        ///     The URI.
        /// </param>
        /// <returns>
        ///     The file-system path, or <c>null</c> if the URI does not represent a file-system path.
        /// </returns>
        public static string GetFileSystemPath(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (uri.Scheme != Uri.UriSchemeFile)
                return null;
            
            // The language server protocol represents "C:\Foo\Bar" as "file:///c:/foo/bar".
            string path = uri.LocalPath;
            if (Path.DirectorySeparatorChar == '\\' && path.StartsWith("/"))
                path = path.Substring(1).Replace('/', '\\');

            return path;
        }
    }
}
