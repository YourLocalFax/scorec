using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    using Logging;
    using Source;

    sealed partial class Parser
    {
        // NOTE(kai): This exists because I might want to multi-thread the compiler.
        // If that's the case, it would be pretty easy to change this to a dictionary
        //  or something and use different parsers for different threads.
        private static Parser parser = null;

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static void Parse(Log log, SourceMap map)
        {
            if (map.Ast != null)
                return;
            if (parser == null)
                parser = new Parser();
            map.Ast = parser.GetAst(log, map);
        }

        /// <summary>
        /// The Log this Lexer will write to.
        /// </summary>
        public Log Log { get; private set; }
        /// <summary>
        /// The SourceFile this Lexer is responsible for lexing.
        /// </summary>
        public SourceMap Map { get; private set; }

        /// <summary>
        /// Save the SourceFile's tokens, I feel like that's not a bad idea.
        /// </summary>
        private List<Token> tokensCached = null;
        /// <summary>
        /// The current index into the token list.
        /// </summary>
        private int tokenOffset = 0;
    }
}
