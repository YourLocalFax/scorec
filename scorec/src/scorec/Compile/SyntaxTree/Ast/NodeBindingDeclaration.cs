namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeBindingDeclaration : Node
    {
        private Span start;
        public override Span Start => start;

        public TokenKind BindingKind;

        public Binding Binding;
        public NodeExpression Value;

        public NodeBindingDeclaration(Span start, TokenKind bindingKind, Binding binding, NodeExpression value)
        {
            this.start = start;
            BindingKind = bindingKind;
            Binding = binding;
            Value = value;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
