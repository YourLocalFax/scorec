namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeIntegerLiteral : NodeExpression
    {
        public override Span Start => TkLiteral.Span;

        /// <summary>
        /// The integer literal token.
        /// </summary>
        public Token TkLiteral;

        public string Literal => TkLiteral.NumericLiteral;
        public int Radix => TkLiteral.IntegerRadix;

        public NodeIntegerLiteral(Token tkLiteral)
        {
            TkLiteral = tkLiteral;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
