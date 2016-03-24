namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeIdentifier : NodeExpression
    {
        public override Span Start => TkIdentifier.Span;

        public Token TkIdentifier;

        public string Identifier => TkIdentifier.Identifier;

        public NodeIdentifier(Token tkIdentifier)
        {
            TkIdentifier = tkIdentifier;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
