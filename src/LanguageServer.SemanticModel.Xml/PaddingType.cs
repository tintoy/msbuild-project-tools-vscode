namespace MSBuildProjectTools.LanguageServer.SemanticModel
{
    /// <summary>
    ///     The type of padding to apply.
    /// </summary>
    public enum PaddingType
    {
        /// <summary>
        ///     No padding.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Add padding before.
        /// </summary>
        Leading = 1,

        /// <summary>
        ///     Add padding after.
        /// </summary>
        Trailing = 2
    }

    /// <summary>
    ///     Extension methods for padding strings.
    /// </summary>
    public static class PaddingExtensions
    {
        /// <summary>
        ///     Add padding to the string.
        /// </summary>
        /// <param name="str">
        ///     The string.
        /// </param>
        /// <param name="paddingType">
        ///     The type of padding to add.
        /// </param>
        /// <returns>
        ///     The padded string.
        /// </returns>
        public static string WithPadding(this string str, PaddingType paddingType)
        {
            if (str == null)
                return null;

            if (paddingType == PaddingType.Leading)
                return " " + str;

            if (paddingType == PaddingType.Trailing)
                return str + " ";

            return str;
        }
    }
}
