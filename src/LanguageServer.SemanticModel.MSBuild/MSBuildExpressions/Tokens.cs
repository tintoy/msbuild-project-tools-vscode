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
        ///     Parse a dollar sign, "$".
        /// </summary>
        public static Parser<char> Dollar = Parse.Char('$');

        /// <summary>
        ///     Parse the opening of an evaluation expression, "$(".
        /// </summary>
        public static Parser<IEnumerable<char>> EvalOpen = Parse.String("$(");

        /// <summary>
        ///     Parse the close of an evaluation expression, ")".
        /// </summary>
        public static Parser<char> EvalClose = Parse.Char(')');

        /// <summary>
        ///     Parse a logical-AND operator, "And".
        /// </summary>
        public static Parser<string> AndOperator = Parse.String("And").Text();

        /// <summary>
        ///     Parse a logical-OR operator, "Or".
        /// </summary>
        public static Parser<string> OrOperator = Parse.String("Or").Text();

        /// <summary>
        ///     Parse a logical-NOT operator, "Not".
        /// </summary>
        public static Parser<string> NotOperator = Parse.String("Not").Text();

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
        ///     Parse a single hexadecimal digit, "[0-9A-F]".
        /// </summary>
        public static Parser<char> HexDigit = Parse.Char(
            predicate: character =>
                (character >= '0' && character <= '9')
                ||
                (character >= 'A' && character <= 'F'),
            description: "hexadecimal digit"
        );

        /// <summary>
        ///     Parse an escaped character, "%xx" (where "x" is a hexadecimal digit, and the resulting number represents the ASCII character code).
        /// </summary>
        public static Parser<char> EscapedChar =
            from escape in Parse.Char('%')
            from hexDigits in HexDigit.Repeat(2).Text()
            select (char)Byte.Parse(hexDigits, NumberStyles.HexNumber);

        /// <summary>
        ///     Parse any character valid within a single-quoted string.
        /// </summary>
        public static Parser<char> SingleQuotedStringChar =
            EscapedChar.Or(
                Parse.AnyChar.Except(
                    SingleQuote.Or(Dollar)
                )
            );

        /// <summary>
        ///     Parse a quoted string.
        /// </summary>
        public static Parser<IEnumerable<char>> QuotedString =
            from leftQuote in SingleQuote
            from stringContents in SingleQuotedStringChar.Many()
            from rightQuote in SingleQuote
            select stringContents;

        /// <summary>
        ///     Parse any character valid within a semicolon-delimited list.
        /// </summary>
        public static Parser<char> ListChar = Parse.AnyChar.Except(Tokens.Semicolon);

        /// <summary>
        ///     Parse a list of strings delimited by semicolons, "ABC;DEF", as character sequences.
        /// </summary>
        public static readonly Parser<IEnumerable<IEnumerable<char>>> DelimitedList = ListChar.Many().DelimitedBy(Semicolon);

        /// <summary>
        ///     Parse a list of strings delimited by semicolons, "ABC;DEF", as strings.
        /// </summary>
        public static readonly Parser<IEnumerable<IEnumerable<char>>> DelimitedListOfStrings = ListChar.Many().Text().DelimitedBy(Semicolon);

        /// <summary>
        ///     Parse an identifier, "ABC".
        /// </summary>
        public static Parser<string> Identifier =
            from first in Parse.Letter
            from rest in Parse.LetterOrDigit.Many().Text()
            select first + rest;

        /// <summary>
        ///     Parse a qualified identifier, "ABC.DEF".
        /// </summary>
        public static Parser<IEnumerable<string>> QualifiedIdentifier = Identifier.DelimitedBy(Period);
    }
}
