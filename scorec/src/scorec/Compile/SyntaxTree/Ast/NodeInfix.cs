namespace ScoreC.Compile.SyntaxTree
{
    using Source;
    using System.Diagnostics;

    class NodeInfix : NodeExpression
    {
        public override Span Start => Left.Start;

        public InfixOperator Operator;

        public NodeExpression Left;
        public NodeExpression Right;

        public NodeInfix(InfixOperator op, NodeExpression left, NodeExpression right)
        {
#if DEBUG
            Debug.Assert(!op.IsAssignment());
#endif
            Operator = op;
            Left = left;
            Right = right;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
