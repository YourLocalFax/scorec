using ScoreC.Compile.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    abstract class Node
    {
        /// <summary>
        /// NOTE(kai): This is called start because dealing with the entire
        ///  surrounding span for every node is going to be a pain.
        /// </summary>
        public abstract Span Start { get; }

        /// <summary>
        /// Used to determine if this is the last piece of code in
        ///  a list of other nodes.
        /// If this is true, the compiler knows this is the last part
        ///  of a set if nodes, which makes knowing where the exit points
        ///  of a procedure are, as well as expression blocks, easy.
        /// </summary>
        public bool InTailPosition = false;

        public abstract void Accept(IAstVisitor visitor);
    }
}
