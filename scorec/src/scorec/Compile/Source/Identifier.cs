using System.Collections.Generic;
using System.Globalization;

namespace ScoreC.Compile.Source
{
    /// <summary>
    /// Provides helper methods for dealing with identifier lexing.
    /// </summary>
    static class Identifier
    {
        private static Dictionary<string, Keyword> KEYWORD_LOOKUP = new Dictionary<string, Keyword>()
        {
            // literal
            { "true", Keyword.True },
            { "false", Keyword.False },
            // operator
            { "as", Keyword.As },
            { "auto", Keyword.Auto },
            // declaration
            { "proc", Keyword.Proc },
            { "type", Keyword.Type },
            { "data", Keyword.Data },
            { "enum", Keyword.Enum },
            { "class", Keyword.Class },
            { "trait", Keyword.Trait },
            { "impl", Keyword.Impl },
            { "mod", Keyword.Mod },
            { "var", Keyword.Var },
            { "let", Keyword.Let },
            { "extern", Keyword.Extern },
            { "export", Keyword.Export },
            // modifier
            { "lazy", Keyword.Lazy },
            { "foreign", Keyword.Foreign },
            { "sealed", Keyword.Sealed },
            { "partial", Keyword.Partial },
            { "pub", Keyword.Pub },
            { "priv", Keyword.Priv },
            { "intern", Keyword.Intern },
            // branch.single
            { "return", Keyword.Return },
            { "break", Keyword.Break },
            { "continue", Keyword.Continue },
            { "goto", Keyword.Goto },
            { "resume", Keyword.Resume },
            { "yield", Keyword.Yield },
            // branch.multiple
            { "if", Keyword.If },
            { "else", Keyword.Else },
            { "unless", Keyword.Unless },
            { "when", Keyword.When },
            { "match", Keyword.Match },
            // branch.loop
            { "while", Keyword.While },
            { "until", Keyword.Until },
            { "loop", Keyword.Loop },
            { "for", Keyword.For },
            { "each", Keyword.Each },
            { "in", Keyword.In },
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
        /// Returns the Keyword that the given identifier represents.
        /// If the identifier does not represent a keyword, returns Keyword.None.
        /// </summary>
        /// <param name="identifier">The possible keyword.</param>
        /// <returns></returns>
        public static Keyword GetKeywordKind(string identifier)
        {
            Keyword result;
            if (!KEYWORD_LOOKUP.TryGetValue(identifier, out result))
                result = Keyword.None;
            return result;
        }

        /// <summary>
        /// Returns true if the given identifier is a keyword.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static bool IsKeyword(string identifier) =>
            GetKeywordKind(identifier) != Keyword.None;
    }
}
