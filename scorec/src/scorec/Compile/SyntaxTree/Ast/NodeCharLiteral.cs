namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeCharLiteral : NodeExpression
    {
        public override Span Start => TkLiteral.Span;

        /// <summary>
        /// The integer literal token.
        /// </summary>
        public Token TkLiteral;

        public uint Literal => TkLiteral.CharacterLiteral;

        public NodeCharLiteral(Token tkLiteral)
        {
            TkLiteral = tkLiteral;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
