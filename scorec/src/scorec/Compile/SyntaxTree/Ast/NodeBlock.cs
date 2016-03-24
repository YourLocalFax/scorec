using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeBlock : NodeExpression
    {
        public override Span Start => TkOpenCurlyBracket.Span;

        public Token TkOpenCurlyBracket;
        public Token TkCloseCurlyBracket;

        public List<Node> Body;

        // FIXME(kai): When we add return statements, include that as an option?
        /// <summary>
        /// CanBeExpression
        /// </summary>
        public bool CanBeExpression => Body != null && Body.Count > 0 && Body[Body.Count - 1] is NodeExpression;

        public NodeBlock(Token tkOpenCurlyBracket, Token tkCloseCurlyBracket, List<Node> body)
        {
            TkOpenCurlyBracket = tkOpenCurlyBracket;
            TkCloseCurlyBracket = tkCloseCurlyBracket;
            Body = body;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
