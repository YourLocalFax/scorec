namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeFieldIndex : NodeExpression
    {
        public override Span Start => Target.Start;

        public Token TkDot;

        public NodeExpression Target;
        public Token TkFieldNameIdentifier;

        public string FieldName => TkFieldNameIdentifier.Identifier;

        public NodeFieldIndex(Token tkDot, NodeExpression target, Token tkFieldNameIdentifier)
        {
            TkDot = tkDot;
            Target = target;
            TkFieldNameIdentifier = tkFieldNameIdentifier;
        }
        
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
