namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeWhileUntil : NodeExpression
    {
        private Span start;
        public override Span Start => start;

        /// <summary>
        /// This will be false for `while` statements and true for `until` statements.
        /// </summary>
        public bool InvertCondition;

        public NodeExpression Condition;
        public NodeExpression Pass;
        public NodeExpression Fail;

        public NodeWhileUntil(Span start, bool invertCondition, NodeExpression condition, NodeExpression pass, NodeExpression fail)
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
