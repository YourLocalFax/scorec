namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeAssignment : Node
    {
        public override Span Start => Target.Start;

        public NodeExpression Target;
        public NodeExpression Value;

        public NodeAssignment(NodeExpression target, NodeExpression value)
        {
            Target = target;
            Value = value;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
