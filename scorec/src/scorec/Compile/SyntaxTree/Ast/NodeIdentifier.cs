namespace ScoreC.Compile.SyntaxTree
{
    using Analysis;
    using Source;

    class NodeIdentifier : NodeExpression
    {
        public override bool IsLValue => true;

        public override Span Start => TkIdentifier.Span;

        public Token TkIdentifier;

        public string Identifier => TkIdentifier.Identifier;

        public Symbol ReferencedSymbol;

        public NodeIdentifier(Token tkIdentifier)
        {
            TkIdentifier = tkIdentifier;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
