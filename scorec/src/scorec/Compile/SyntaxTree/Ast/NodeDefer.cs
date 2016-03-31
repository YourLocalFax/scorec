namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeDefer : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public Node Target;

        public NodeDefer(Span start, Node target)
        {
            this.start = start;
            Target = target;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
