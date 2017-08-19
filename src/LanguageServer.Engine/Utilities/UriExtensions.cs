using System;
using System.IO;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Helper methods for <see cref="Uri"/>s.
    /// </summary>
    static class UriHelper
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
            string fileSystemPath = Uri.UnescapeDataString(uri.AbsolutePath);
            if (Path.DirectorySeparatorChar == '\\')
            {
                if (fileSystemPath.StartsWith("/"))
                    fileSystemPath = fileSystemPath.Substring(1);

                fileSystemPath = fileSystemPath.Replace('/', '\\');
            }

            return fileSystemPath;
        }

        /// <summary>
        ///     Convert a file-system path to a VSCode document URI.
        /// </summary>
        /// <param name="fileSystemPath">
        ///     The file-system path.
        /// </param>
        /// <returns>
        ///     The VSCode document URI.
        /// </returns>
        public static Uri CreateDocumentUri(string fileSystemPath)
        {
            if (String.IsNullOrWhiteSpace(fileSystemPath))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'fileSystemPath'.", nameof(fileSystemPath));
            
            if (!Path.IsPathRooted(fileSystemPath))
                throw new ArgumentException($"Path '{fileSystemPath}' is not an absolute path.", nameof(fileSystemPath));

            if (Path.DirectorySeparatorChar == '\\')
                fileSystemPath = fileSystemPath.Replace('\\', '/');

            UriBuilder documentUriBuilder = new UriBuilder
            {
                Scheme = Uri.UriSchemeFile,
                Host = "", // Needed to get the leading triple-slash in the URI
                Path = fileSystemPath
            };

            return documentUriBuilder.Uri;
        }
    }
}
