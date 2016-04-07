using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using System.Linq;
    using Source;

    class NodeBlock : NodeExpression
    {
        public Span TkOpenCurlyBraceStart;
        public override Span Start => TkOpenCurlyBraceStart;

        public List<Node> Body;

        public bool CanBeExpression = true;
        public bool CreateScope = true;

        public NodeBlock(Span tkOpenCurlyBraceStart, List<Node> body)
        {
            TkOpenCurlyBraceStart = tkOpenCurlyBraceStart;
            Body = body;

            // NOTE(kai): IsResultRequired cannot be properly set for the body because we don't yet know if this block requires a result.

            body.Last().InTailPosition = true;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
