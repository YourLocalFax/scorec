namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class Binding
    {
        public Token TkName;
        public string Name => TkName.Identifier;

        public TypeInfo DeclaredType;
        public bool ShouldBeTypeInferred => DeclaredType == null;

        public NodeExpression Value;

        public Binding(Token tkName, TypeInfo declaredType, NodeExpression value)
        {
            TkName = tkName;
            DeclaredType = declaredType;
            Value = value;
        }

        public override string ToString() =>
            TkName == null ? DeclaredType.ToString() : (DeclaredType == null ? Name : string.Format("{0}: {1}", Name, DeclaredType.ToString()));
    }
}
