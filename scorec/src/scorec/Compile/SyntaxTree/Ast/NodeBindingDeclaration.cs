using System.Diagnostics;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeBindingDeclaration : Node
    {
        private Span start;
        public override Span Start => start;

        public TokenKind BindingKind;

        public Binding Binding;
        public NodeExpression Value => Binding.Value;

        public NodeBindingDeclaration(Span start, TokenKind bindingKind, Binding binding)
        {
#if DEBUG
            Debug.Assert(bindingKind == TokenKind.Var || bindingKind == TokenKind.Let);
#endif
            this.start = start;
            BindingKind = bindingKind;
            Binding = binding;

            if (Binding.Value != null)
                Binding.Value.IsResultRequired = true;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
