namespace ScoreC.Compile.SyntaxTree
{
    using System.Diagnostics;
    using Source;

    class NodeLoad : Node
    {
        private Span start;
        public override Span Start => start;

        public string LoadPath;

        public NodeLoad(Span start, string loadPath)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(loadPath));
#endif
            this.start = start;
            LoadPath = loadPath;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
