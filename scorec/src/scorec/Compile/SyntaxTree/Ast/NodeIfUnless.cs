namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeIfUnless : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        /// <summary>
        /// This will be false for `if` statements and true for `unless` statements.
        /// </summary>
        public bool InvertCondition;

        public NodeExpression Condition;
        public NodeExpression Pass;
        public NodeExpression Fail;

        public NodeIfUnless(Span start, bool invertCondition, NodeExpression condition, NodeExpression pass, NodeExpression fail)
        {
            this.start = start;
            InvertCondition = invertCondition;
            Condition = condition;
            Pass = pass;
            Fail = fail;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
