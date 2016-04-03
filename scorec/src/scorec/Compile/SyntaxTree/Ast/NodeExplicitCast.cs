namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeExplicitCast : NodeExpression
    {
        public override Span Start => Target.Start;

        public NodeExpression Target;

        public TypeInfo CastTo;

        public NodeExplicitCast(NodeExpression target, TypeInfo castTo)
        {
            Target = target;
            CastTo = castTo;

            TypeInfo = CastTo;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
