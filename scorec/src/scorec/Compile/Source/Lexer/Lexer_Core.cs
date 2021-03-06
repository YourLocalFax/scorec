﻿using System.Diagnostics;
using System.Text;

namespace ScoreC.Compile.Source
{
    using Logging;

    sealed partial class Lexer
    {
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static void Lex(Log log, SourceMap file)
        {
            var lexer = new Lexer(log);
            if (file.Tokens != null)
                return;
            file.Tokens = lexer.GetTokens(file);
        }

        /// <summary>
        /// The Log this Lexer will write to.
        /// </summary>
        private Log log;
        /// <summary>
        /// The SourceFile this Lexer is responsible for lexing.
        /// </summary>
        private SourceMap map;
        /// <summary>
        /// Save the SourceFile's source, I feel like that's not a bad idea.
        /// </summary>
        private string sourceCached = null;
        /// <summary>
        /// The current index into the source string.
        /// </summary>
        private int sourceOffset = 0;

        /// <summary>
        /// The current line our "cursor" is at.
        /// </summary>
        private int line = 0;
        /// <summary>
        /// The current column our "cursor" is at.
        /// </summary>
        private int column = 0;

        /// <summary>
        /// Used to easily construct strings from the source characters.
        /// </summary>
        private readonly StringBuilder buffer = new StringBuilder();

        private Lexer(Log log)
        {
            this.log = log;
        }

        /// <summary>
        /// Returns a Span covering the current character only.
        /// </summary>
        private Span GetSpan() =>
            new Span(map, line, column);

        /// <summary>
        /// Returns a Span starting at the given Span and ending at the current character.
        /// </summary>
        /// <param name="start">The start Span.</param>
        private Span GetSpan(Span start) =>
            new Span(map, start.Line, start.Column, line, column);

        /// <summary>
        /// Determines if the given string is present in the source at the current offset.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool Matches(string s)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(s), "Match string cannot be null or empty.");
#endif
            if (IsEndOfSource)
                return false;
            var len = s.Length;
            if (sourceOffset + len >= sourceCached.Length)
                return false;
            for (var i = 0; i < len; i++)
                if (s[i] != sourceCached[sourceOffset + i])
                    return false;
            return true;
        }
    }
}
