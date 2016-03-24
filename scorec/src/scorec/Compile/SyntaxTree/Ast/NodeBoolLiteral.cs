namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeBoolLiteral : NodeExpression
    {
        public override Span Start => TkLiteral.Span;

        /// <summary>
        /// The integer literal token.
        /// </summary>
        public Token TkLiteral;

        public bool Value => TkLiteral.Bool;

        public NodeBoolLiteral(Token tkLiteral)
        {
            TkLiteral = tkLiteral;
            // Bools are always of bool type, so set that.
            TypeInfo = BuiltinTypeInfo.Get(BuiltinType.BOOL);
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
