namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    sealed class PointerTypeInfo : TypeInfo
    {
        public Token Star;
        public TypeInfo PointerTo;

        public PointerTypeInfo(Token star, TypeInfo pointerTo)
        {
            Star = star;
            PointerTo = pointerTo;
        }

        public override string ToString() =>
            string.Format("*{0}", PointerTo.ToString());
    }
}
