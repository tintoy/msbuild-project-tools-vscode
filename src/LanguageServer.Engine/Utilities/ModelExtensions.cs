using System;
using Microsoft.Build.Construction;

using MLXML = Microsoft.Language.Xml;

namespace MSBuildProjectTools.LanguageServer.Utilities
{
    /// <summary>
    ///     Extension methods for converting models between native and third-party representations.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        ///     Convert the <see cref="Position"/> to its Language Server Protocol equivalent.
        /// </summary>
        /// <param name="position">
        ///     The <see cref="Position"/> to convert.
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Lsp.Models.Position"/>.
        /// </returns>
        public static Lsp.Models.Position ToLsp(this Position position)
        {
            if (position == null)
                return null;

            position = position.ToZeroBased(); // LSP is zero-based.

            return new Lsp.Models.Position(
                position.LineNumber,
                position.ColumnNumber
            );
        }

        /// <summary>
        ///     Convert the Language Server Protocol <see cref="Lsp.Models.Position"/> to its native equivalent.
        /// </summary>
        /// <param name="position">
        ///     The <see cref="Lsp.Models.Position"/> to convert.
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Position"/>.
        /// </returns>
        public static Position ToNative(this Lsp.Models.Position position)
        {
            if (position == null)
                return null;

            // LSP is zero-based.
            return Position.FromZeroBased(
                position.Line,
                position.Character
            ).ToOneBased();
        }

        /// <summary>
        ///     Convert the MSBuild <see cref="MSBuild.ElementPosition"/> to its native equivalent.
        /// </summary>
        /// <param name="location">
        ///     The <see cref="MSBuild.ElementPosition"/> to convert.
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Position"/>.
        /// </returns>
        public static Position ToNative(this ElementLocation location)
        {
            if (location == null)
                return null;

            return new Position(location.Line, location.Column);
        }

        /// <summary>
        ///     Convert the <see cref="Range"/> to its Language Server Protocol equivalent.
        /// </summary>
        /// <param name="position">
        ///     The <see cref="Range"/> to convert.
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Lsp.Models.Range"/>.
        /// </returns>
        public static Lsp.Models.Range ToLsp(this Range range)
        {
            if (range == null)
                return null;

            return new Lsp.Models.Range(
                range.Start.ToLsp(),
                range.End.ToLsp()
            );
        }

        /// <summary>
        ///     Convert the <see cref="MXML.TextRange"/> to its native equivalent.
        /// </summary>
        /// <param name="span">
        ///     The <see cref="MXML.TextSpan"/> to convert.
        /// </param>
        /// <param name="textPositions">
        ///     The textual position lookup used to map absolute positions to lines and columns.
        /// </span>
        /// <returns>
        ///     The equivalent <see cref="Range"/>.
        /// </returns>
        public static Range ToNative(this MLXML.TextSpan span, TextPositions textPositions)
        {
            if (textPositions == null)
                throw new ArgumentNullException(nameof(textPositions));

            return new Range(
                start: textPositions.GetPosition(span.Start),
                end: textPositions.GetPosition(span.End)
            );
        }

        /// <summary>
        ///     Convert the Language Server Protocol <see cref="Lsp.Models.Range"/> to its native equivalent.
        /// </summary>
        /// <param name="position">
        ///     The <see cref="Lsp.Models.Range"/> to convert.
        /// </param>
        /// <returns>
        ///     The equivalent <see cref="Range"/>.
        /// </returns>
        public static Range ToNative(this Lsp.Models.Range range)
        {
            if (range == null)
                return null;

            return new Range(
                range.Start.ToNative(),
                range.End.ToNative()
            );
        }
    }
}
