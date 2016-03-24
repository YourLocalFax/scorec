namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeFieldIndex : NodeExpression
    {
        public override Span Start => Target.Start;

        public NodeExpression Target;
        public Token TkFieldNameIdentifier;

        public string FieldName => TkFieldNameIdentifier.Identifier;

        public NodeFieldIndex(NodeExpression target, Token tkFieldNameIdentifier)
        {
            Target = target;
            TkFieldNameIdentifier = tkFieldNameIdentifier;
        }
        
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
