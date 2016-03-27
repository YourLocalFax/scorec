namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeBindingDeclaration : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        public Keyword BindingKind;

        public Token TkIdentifier;
        public string BindingName => TkIdentifier.Identifier;

        public TypeInfo DeclaredType;
        public bool ShouldBeTypeInferred => DeclaredType == null;

        public NodeExpression Value;

        public NodeBindingDeclaration(Span start, Keyword bindingKind, Token tkIdentifier, TypeInfo declaredType, NodeExpression value)
        {
            this.start = start;
            BindingKind = bindingKind;
            TkIdentifier = tkIdentifier;
            DeclaredType = declaredType;
            Value = value;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
