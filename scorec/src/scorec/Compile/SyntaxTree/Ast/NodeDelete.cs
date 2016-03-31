namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeDelete : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public NodeExpression Target;

        public NodeDelete(Span start, NodeExpression target)
        {
            this.start = start;
            Target = target;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
