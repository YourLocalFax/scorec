namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeRealLiteral : NodeExpression
    {
        public override Span Start => TkLiteral.Span;

        /// <summary>
        /// The integer literal token.
        /// </summary>
        public Token TkLiteral;

        public string Literal => TkLiteral.NumericLiteral;

        public NodeRealLiteral(Token tkLiteral)
        {
            TkLiteral = tkLiteral;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
