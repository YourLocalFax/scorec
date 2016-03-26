namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeCharLiteral : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public uint Literal;

        public NodeCharLiteral(Span start, uint literal)
        {
            this.start = start;
            Literal = literal;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
