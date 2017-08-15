namespace MSBuildProjectTools.LanguageServer.Utilities
{
    using XmlParser;

    public static class ModelExtensions
    {
        public static Lsp.Models.Position ToLspModel(this Position position)
        {
            if (position == null)
                return null;

            position = position.ToZeroBased(); // LSP is zero-based.

            return new Lsp.Models.Position(
                position.LineNumber,
                position.ColumnNumber
            );
        }

        public static Lsp.Models.Range ToLspModel(this Range range)
        {
            if (range == null)
                return null;

            return new Lsp.Models.Range(
                range.Start.ToLspModel(),
                range.End.ToLspModel()
            );
        }
    }
}
