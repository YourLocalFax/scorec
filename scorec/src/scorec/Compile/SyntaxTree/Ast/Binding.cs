namespace ScoreC.Compile.SyntaxTree
{
    using Analysis;
    using Source;

    class Binding
    {
        public Token TkName;
        public string Name => TkName?.Identifier;

        public TypeInfo DeclaredTypeInfo;
        public bool ShouldBeTypeInferred => DeclaredTypeInfo == null;
        public Span TypeStart;

        public Symbol TypeSymbol;
        public TypeInfo TypeInfo => TypeSymbol.TypeInfo;

        public NodeExpression Value;

        public Binding(Token tkName, TypeInfo declaredType, Span typeStart, NodeExpression value)
        {
            TkName = tkName;
            DeclaredTypeInfo = declaredType;
            TypeStart = typeStart;
            Value = value;
        }

        public override string ToString() =>
            TkName == null ? DeclaredTypeInfo.ToString() : (DeclaredTypeInfo == null ? Name : string.Format("{0}: {1}", Name, DeclaredTypeInfo.ToString()));
    }
}
