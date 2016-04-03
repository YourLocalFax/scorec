namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class Binding
    {
        public Token TkName;
        public string Name => TkName.Identifier;

        public TypeInfo DeclaredTypeInfo;
        public bool ShouldBeTypeInferred => DeclaredTypeInfo == null;

        public NodeExpression Value;

        public Binding(Token tkName, TypeInfo declaredType, NodeExpression value)
        {
            TkName = tkName;
            DeclaredTypeInfo = declaredType;
            Value = value;
        }

        public override string ToString() =>
            TkName == null ? DeclaredTypeInfo.ToString() : (DeclaredTypeInfo == null ? Name : string.Format("{0}: {1}", Name, DeclaredTypeInfo.ToString()));
    }
}
