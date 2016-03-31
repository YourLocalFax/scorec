using System.Collections.Generic;
using System.Globalization;

namespace ScoreC.Compile.Source
{
    /// <summary>
    /// Provides helper methods for dealing with identifier lexing.
    /// </summary>
    static class Identifier
    {
        private static Dictionary<string, TokenKind> KEYWORD_LOOKUP = new Dictionary<string, TokenKind>()
        {
            // literal
            { "true", TokenKind.True },
            { "false", TokenKind.False },
            // operator
            { "is", TokenKind.Is },
            { "as", TokenKind.As },
            { "auto", TokenKind.Auto },
            { "new", TokenKind.New },
            { "delete", TokenKind.Delete },
            // declaration
            { "proc", TokenKind.Proc },
            { "type", TokenKind.Type },
            { "struct", TokenKind.Struct },
            { "mod", TokenKind.Mod },
            { "var", TokenKind.Var },
            { "let", TokenKind.Let },
            { "extern", TokenKind.Extern },
            { "export", TokenKind.Export },
            // modifier
            { "lazy", TokenKind.Lazy },
            { "foreign", TokenKind.Foreign },
            { "sealed", TokenKind.Sealed },
            { "partial", TokenKind.Partial },
            { "pub", TokenKind.Pub },
            { "priv", TokenKind.Priv },
            { "intern", TokenKind.Intern },
            // branch.single
            { "return", TokenKind.Return },
            { "break", TokenKind.Break },
            { "continue", TokenKind.Continue },
            { "goto", TokenKind.Goto },
            { "resume", TokenKind.Resume },
            { "yield", TokenKind.Yield },
            // branch.multiple
            { "if", TokenKind.If },
            { "else", TokenKind.Else },
            { "unless", TokenKind.Unless },
            { "when", TokenKind.When },
            { "match", TokenKind.Match },
            // branch.loop
            { "while", TokenKind.While },
            { "until", TokenKind.Until },
            { "loop", TokenKind.Loop },
            { "for", TokenKind.For },
            { "each", TokenKind.Each },
            { "in", TokenKind.In },
        };

        /// <summary>
        /// Returns true for unicode characters of classes Lu, Ll, Lt, Lm, Lo, or Nl.
        /// </summary>
        private static bool IsLetterChar(int c)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(c), 0);
            switch (cat)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Returns true for unicode characters of classes Mn or Mc.
        /// </summary>
        private static bool IsCombiningChar(int c)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(c), 0);
            switch (cat)
            {
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.NonSpacingMark:
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Returns true for unicode characters of class Nd.
        /// </summary>
        private static bool IsDecimalDigitChar(int c) =>
            CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(c), 0) == UnicodeCategory.DecimalDigitNumber;

        /// <summary>
        /// Returns true for unicode characters of class Pc.
        /// </summary>
        private static bool IsConnectingChar(int c) =>
            CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(c), 0) == UnicodeCategory.ConnectorPunctuation;

        /// <summary>
        /// Returns true for unicode characters of class Cf.
        /// </summary>
        private static bool IsFormattingChar(int c) =>
            CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(c), 0) == UnicodeCategory.Format;

        /// <summary>
        /// Returns true if the unicode character is a letter char or an underscore (U+005F).
        /// </summary>
        public static bool IsStart(int c) =>
            IsLetterChar(c) || c == '_';

        /// <summary>
        /// Returns true if the unicode character is an identifier start or
        /// a combining, decimal digit, connecting, or formatting char.
        /// </summary>
        public static bool IsPart(int c) =>
            IsStart(c) || IsCombiningChar(c) || IsDecimalDigitChar(c) || IsConnectingChar(c) || IsFormattingChar(c);

        /// <summary>
        /// Returns true if the given string represents a valid Score identifier.
        /// This will return false for the wildcard character ('_').
        /// </summary>
        /// <param name="identifier">The possible identifier.</param>
        /// <returns></returns>
        public static bool IsValid(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) || identifier == "_" || !IsStart(identifier[0]))
                return false;
            for (var i = 1; i < identifier.Length; i++)
                if (!IsPart(identifier[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Returns the TokenKind that the given identifier represents.
        /// If the identifier does not represent a keyword, returns TokenKind.None.
        /// </summary>
        /// <param name="identifier">The possible TokenKind.</param>
        /// <returns></returns>
        public static TokenKind GetKeywordKind(string identifier)
        {
            TokenKind result;
            if (!KEYWORD_LOOKUP.TryGetValue(identifier, out result))
                result = TokenKind.None;
            return result;
        }

        /// <summary>
        /// Returns true if the given identifier is a keyword.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static bool IsKeyword(string identifier) =>
            GetKeywordKind(identifier) != TokenKind.None;
    }
}
