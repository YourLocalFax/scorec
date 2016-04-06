using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Logging;
    using Source;

    sealed partial class Parser
    {
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static void Parse(Project project, SourceMap file)
        {
            var parser = new Parser(project);
            if (file.Ast != null)
                return;
            file.Ast = parser.GetAst(file);
        }

        private Project project;
        /// <summary>
        /// The Log this Lexer will write to.
        /// </summary>
        private Log log => project.Log;
        /// <summary>
        /// The SourceFile this Lexer is responsible for lexing.
        /// </summary>
        private SourceMap map;
        /// <summary>
        /// Save the SourceFile's tokens, I feel like that's not a bad idea.
        /// </summary>
        private List<Token> tokensCached = null;
        /// <summary>
        /// The current index into the token list.
        /// </summary>
        private int tokenOffset = 0;

        private Parser(Project project)
        {
            this.project = project;
        }
    }
}
