using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeStructDeclaration : Node
    {
        private Span start;
        public override Span Start => start;

        public NodeStructDeclaration(Span start, Token tkName, List<Binding> fields)
        {
            this.start = start;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
