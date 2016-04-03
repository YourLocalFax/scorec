using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    using Analysis;
    using Source;

    sealed class Ast
    {
        public SourceMap Map { get; private set; }
        private readonly List<Node> nodes = new List<Node>();

        public Ast(SourceMap map)
        {
            Map = map;
        }

        /// <summary>
        /// Places the given node at the end of this AST.
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(Node node)
        {
            nodes.Add(node);
        }

        // TODO(kai): Insert before/after another node (for compile-time code modification).

        public void Accept(IAstVisitor visitor)
        {
            nodes.ForEach(node => node.Accept(visitor));
        }
    }
}
