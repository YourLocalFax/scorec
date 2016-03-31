namespace ScoreC.Compile.SyntaxTree
{
    using Source;
    using System.Diagnostics;

    class NodePrefix : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public PrefixOperator Operator;

        public NodeExpression Target;

        public NodePrefix(Span start, PrefixOperator op, NodeExpression target)
        {
            this.start = start;
            Operator = op;
            Target = target;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
