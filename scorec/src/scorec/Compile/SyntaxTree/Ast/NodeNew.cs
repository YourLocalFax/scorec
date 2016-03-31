namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeNew : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public TypeInfo Type;

        public NodeNew(Span start, TypeInfo type)
        {
            this.start = start;
            Type = type;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
