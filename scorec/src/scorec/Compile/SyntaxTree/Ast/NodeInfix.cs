namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeInfix : NodeExpression
    {
        public override Span Start => Left.Start;

        public OperatorKind Operator;

        public NodeExpression Left;
        public NodeExpression Right;

        public NodeInfix(OperatorKind op, NodeExpression left, NodeExpression right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
