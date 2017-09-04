using Sprache;
using System;
using System.Collections.Generic;

namespace MSBuildProjectTools.LanguageServer.SemanticModel.MSBuildExpressions
{
    /// <summary>
    ///     Token parsers for MSBuild expression syntax.
    /// </summary>
    static class Tokens
    {
        /// <summary>
        ///     Parse a semicolon, ";".
        /// </summary>
        public static Parser<char> Semicolon = Parse.Char(';');

        /// <summary>
        ///     Parse a period, ".".
        /// </summary>
        public static Parser<char> Period = Parse.Char('.');

        /// <summary>
        ///     Parse a single quote, "'".
        /// </summary>
        public static Parser<char> SingleQuote = Parse.Char('\'');

        /// <summary>
        ///     Parse an escaped char, "\x" (where "x" is the escaped char).
        /// </summary>
        public static Parser<char> EscapedChar = Parse.Char('\'').Then(_ => Parse.AnyChar);

        /// <summary>
        ///     Parse any character valid within a single-quoted string.
        /// </summary>
        public static Parser<char> SingleQuotedStringChar = EscapedChar.Or(Parse.AnyChar.Except(Tokens.SingleQuote));

        /// <summary>
        ///     Parse a quoted string.
        /// </summary>
        public static Parser<IEnumerable<char>> QuotedString = SingleQuotedStringChar.DelimitedBy(SingleQuote);

        /// <summary>
        ///     Parse any character valid within a semicolon-delimited list.
        /// </summary>
        public static Parser<char> ListChar = Parse.AnyChar.Except(Tokens.Semicolon);

        /// <summary>
        ///     Parse a list of strings delimited by semicolons.
        /// </summary>
        public static readonly Parser<IEnumerable<IEnumerable<char>>> DelimitedList = ListChar.Many().DelimitedBy(Tokens.Semicolon);

        /// <summary>
        ///     Parse a list of strings delimited by semicolons.
        /// </summary>
        public static readonly Parser<IEnumerable<IEnumerable<char>>> DelimitedListOfStrings = ListChar.Many().Text().DelimitedBy(Tokens.Semicolon);

        /// <summary>
        ///     Parse an identifier.
        /// </summary>
        public static Parser <string> Identifier =
            from first in Parse.Letter
            from rest in Parse.LetterOrDigit.Many().Text()
            select first + rest;
    }
}
