namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeAutoCast : NodeExpression
    {
        public override Span Start => Target.Start;

        public NodeExpression Target;

        public NodeAutoCast(NodeExpression target)
        {
            Target = target;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
