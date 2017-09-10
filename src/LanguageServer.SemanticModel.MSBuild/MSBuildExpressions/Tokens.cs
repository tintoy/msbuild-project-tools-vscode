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
        ///     Parse a period, ".".
        /// </summary>
        public static Parser<char> Period = Parse.Char('.').Named("token: period");

        /// <summary>
        ///     Parse a period, ",".
        /// </summary>
        public static Parser<char> Comma = Parse.Char(',').Named("token: comma");

        /// <summary>
        ///     Parse a dollar sign, "$".
        /// </summary>
        public static Parser<char> Dollar = Parse.Char('$').Named("token: dollar");

        /// <summary>
        ///     Parse an at sign, "@".
        /// </summary>
        public static Parser<char> At = Parse.Char('@').Named("token: at");

        /// <summary>
        ///     Parse a colon, ":".
        /// </summary>
        public static Parser<char> Colon = Parse.Char(':').Named("token: colon");

        /// <summary>
        ///     Parse a semicolon, ";".
        /// </summary>
        public static Parser<char> Semicolon = Parse.Char(';').Named("token: semicolon");

        /// <summary>
        ///     Parse a left parenthesis, "(".
        /// </summary>
        public static Parser<char> LParen = Parse.Char('(').Named("token: left parenthesis");

        /// <summary>
        ///     Parse a right parenthesis, ")".
        /// </summary>
        public static Parser<char> RParen = Parse.Char(')').Named("token: right parenthesis");

        /// <summary>
        ///     Parse a left bracket, "[".
        /// </summary>
        public static Parser<char> LBracket = Parse.Char('[').Named("token: left bracket");

        /// <summary>
        ///     Parse a right bracket, "]".
        /// </summary>
        public static Parser<char> RBracket = Parse.Char(']').Named("token: right bracket");

        /// <summary>
        ///     Parse the opening of an evaluation expression, "$(".
        /// </summary>
        public static Parser<IEnumerable<char>> EvalOpen = Parse.String("$(").Named("token: eval open");

        /// <summary>
        ///     Parse the close of an evaluation expression, ")".
        /// </summary>
        public static Parser<char> EvalClose = RParen.Named("token: eval close");

        /// <summary>
        ///     Parse the opening of an item group expression, "@(".
        /// </summary>
        public static Parser<IEnumerable<char>> ItemGroupOpen = Parse.String("@(").Named("token: item group open");

        /// <summary>
        ///     Parse the close of an item group expression, ")".
        /// </summary>
        public static Parser<char> ItemGroupClose = RParen.Named("token: item group close");

        /// <summary>
        ///     Parse a logical-AND operator, "And".
        /// </summary>
        public static Parser<string> AndOperator = Parse.String("And").Text().Named("token: logical-AND operator");

        /// <summary>
        ///     Parse a logical-OR operator, "Or".
        /// </summary>
        public static Parser<string> OrOperator = Parse.String("Or").Text().Named("token: logical-OR operator");

        /// <summary>
        ///     Parse a logical-NOT operator, "!".
        /// </summary>
        public static Parser<string> NotOperator = Parse.String("!").Text().Named("token: logical-NOT operator");

        /// <summary>
        ///     Parse an equality operator, "==".
        /// </summary>
        public static Parser<string> EqualityOperator = Parse.String("==").Text().Named("token: equality operator");

        /// <summary>
        ///     Parse an inequality operator, "!=".
        /// </summary>
        public static Parser<string> InequalityOperator = Parse.String("!=").Text().Named("token: inequality operator");

        /// <summary>
        ///     Parse a single quote, "'".
        /// </summary>
        public static Parser<char> SingleQuote = Parse.Char('\'').Named("token: single quote");

        /// <summary>
        ///     Parse a single hexadecimal digit, "[0-9A-F]".
        /// </summary>
        public static Parser<char> HexDigit = Parse.Char(
            predicate: character =>
                (character >= '0' && character <= '9')
                ||
                (character >= 'A' && character <= 'F'),
            
            description: "token: hexadecimal digit"
        );

        /// <summary>
        ///     Parse an escaped character, "%xx" (where "x" is a hexadecimal digit, and the resulting number represents the ASCII character code).
        /// </summary>
        public static Parser<char> EscapedChar = Parse.Named(
            from escape in Parse.Char('%')
            from hexDigits in HexDigit.Repeat(2).Text()
            select (char)Byte.Parse(hexDigits, NumberStyles.HexNumber),
            
            name: "token: escaped character"
        );

        /// <summary>
        ///     Parse any character valid within a single-quoted string.
        /// </summary>
        public static Parser<char> SingleQuotedStringChar =
            EscapedChar.Or(
                Parse.AnyChar.Except(
                    SingleQuote.Or(Dollar).Or(At) // FIXME: Technically these should be EvalOpen and ItemGroupOpen; a single "$" or "@" is legal.
                )
            ).Named("token: single-quoted string character");

        /// <summary>
        ///     Parse a quoted string.
        /// </summary>
        public static Parser<IEnumerable<char>> QuotedString = Parse.Named(
            from leftQuote in SingleQuote
            from stringContents in SingleQuotedStringChar.Many()
            from rightQuote in SingleQuote
            select stringContents,
            
            name: "token: quoted string"
        );

        /// <summary>
        ///     Parse any character valid within a semicolon-delimited list.
        /// </summary>
        public static Parser<char> ListChar = Parse.AnyChar.Except(Semicolon).Named("token: list character");

        /// <summary>
        ///     Parse a list of strings delimited by semicolons, "ABC;DEF", as character sequences.
        /// </summary>
        public static readonly Parser<IEnumerable<IEnumerable<char>>> DelimitedList = ListChar.Many().DelimitedBy(Semicolon).Named("token: delimited list");

        /// <summary>
        ///     Parse a list of strings delimited by semicolons, "ABC;DEF", as strings.
        /// </summary>
        public static readonly Parser<IEnumerable<IEnumerable<char>>> DelimitedListOfStrings = ListChar.Many().Text().DelimitedBy(Semicolon).Named("token: delimited string list");

        /// <summary>
        ///     Parse an identifier, "ABC".
        /// </summary>
        public static Parser<string> Identifier = Parse.Named(
            from first in Parse.Letter
            from rest in Parse.LetterOrDigit.Many().Text()
            select first + rest,
            
            name: "token: identifier"
        );

        /// <summary>
        ///     Parse a qualified identifier, "ABC.DEF".
        /// </summary>
        public static Parser<IEnumerable<string>> QualifiedIdentifier = Identifier.DelimitedBy(Period).Named("token: qualified identifier");
    }
}
