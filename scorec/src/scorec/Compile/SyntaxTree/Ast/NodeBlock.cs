using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeBlock : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public List<Node> Body;

        // FIXME(kai): When we add return statements, include that as an option?
        /// <summary>
        /// CanBeExpression
        /// </summary>
        public bool CanBeExpression => Body != null && Body.Count > 0 && Body[Body.Count - 1] is NodeExpression;

        public NodeBlock(Span start, List<Node> body)
        {
            this.start = start;
            Body = body;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
