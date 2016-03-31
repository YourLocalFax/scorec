namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeOperatorAssignment : Node
    {
        public override Span Start => Target.Start;

        public InfixOperator Operator;

        public NodeExpression Target;
        public NodeExpression Value;

        public NodeOperatorAssignment(InfixOperator op, NodeExpression target, NodeExpression value)
        {
            Operator = op;
            Target = target;
            Value = value;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
