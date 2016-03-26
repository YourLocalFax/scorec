namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeBindingDeclaration : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public Token TkIdentifier;
        public string BindingName => TkIdentifier.Identifier;

        public NodeExpression Value;

        public NodeBindingDeclaration(Token tkIdentifier, NodeExpression value)
        {
            TkIdentifier = tkIdentifier;
            Value = value;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
