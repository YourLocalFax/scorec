using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    using System.Threading;
    using Logging;
    using Source;

    sealed partial class Parser
    {
        private static List<Thread> activeThreads = new List<Thread>();

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static void Parse(SourceMap map)
        {
            if (map.Ast != null)
                return;
            /*
            var thread = new Thread(() =>
            {
                var parser = new Parser();
                map.Ast = parser.GetAst(Project.Log, map);
            });
            activeThreads.Add(thread);
            thread.Start();
            */
            var parser = new Parser();
            map.Ast = parser.GetAst(Project.Log, map);
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
