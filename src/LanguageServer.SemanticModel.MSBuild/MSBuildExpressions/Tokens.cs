using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;

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
        ///     Parse an equality operator, "==".
        /// </summary>
        public static Parser<string> EqualityOperator = Parse.String("==").Text();

        /// <summary>
        ///     Parse an inequality operator, "!=".
        /// </summary>
        public static Parser<string> InequalityOperator = Parse.String("!=").Text();

        /// <summary>
        ///     Parse a single quote, "'".
        /// </summary>
        public static Parser<char> SingleQuote = Parse.Char('\'');

        /// <summary>
        ///     Parse a single hexadecimal digit, "[0-9A-Fa-f]".
        /// </summary>
        public static Parser<char> HexDigit = Parse.Chars('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f');

        /// <summary>
        ///     Parse an escaped char, "%xx" (where "x" is a hexadecimal digit).
        /// </summary>
        public static Parser<char> EscapedChar =
            from escape in Parse.Char('%')
            from hex in HexDigit.Repeat(2).Text()
            let character = (char)Byte.Parse(hex, NumberStyles.HexNumber)
            select (char)Byte.Parse(hex);

        /// <summary>
        ///     Parse any character valid within a single-quoted string.
        /// </summary>
        public static Parser<char> SingleQuotedStringChar = EscapedChar.Or(Parse.AnyChar.Except(SingleQuote));

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
