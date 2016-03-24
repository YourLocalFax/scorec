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

        public abstract void Accept(IAstVisitor visitor);
    }
}
