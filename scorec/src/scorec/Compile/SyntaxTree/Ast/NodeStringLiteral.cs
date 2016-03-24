namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeStringLiteral : NodeExpression
    {
        public override Span Start => TkLiteral.Span;

        /// <summary>
        /// The integer literal token.
        /// </summary>
        public Token TkLiteral;

        public string Literal => TkLiteral.StringLiteral;

        public NodeStringLiteral(Token tkLiteral)
        {
            TkLiteral = tkLiteral;
            // FIXME(kai): Eventually we'll actually infer the type of strings, but for now it's always a *s8
            TypeInfo = new PointerTypeInfo(null, BuiltinTypeInfo.Get(BuiltinType.S8));
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
