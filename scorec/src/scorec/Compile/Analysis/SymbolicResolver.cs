namespace ScoreC.Compile.Analysis
{
    using Logging;
    using Source;
    using SyntaxTree;

    class SymbolicResolver : IAstVisitor
    {
        public static void Resolve(Project project)
        {
            var walker = new SymbolTableWalker(project.SymbolTable);
            foreach (var file in project.Files)
            {
                var resolver = new SymbolicResolver(project, file, walker);
                resolver.Resolve();
            }
        }

        private Project project;
        private SourceMap file;
        private SymbolTableWalker walker;

        private Log log => project.Log;

        private bool isFinished = true;

        private SymbolicResolver(Project project, SourceMap file, SymbolTableWalker walker)
        {
            this.project = project;
            this.file = file;
            this.walker = walker;
        }

        private void Resolve()
        {
            do
            {
                isFinished = true;
                file.Ast.Accept(this);
            }
            while (!isFinished);
        }

        private Symbol GetReferencedSymbol(string ident)
        {
            var refersTo = walker.SymbolTable.FindGlobal(ident);

            if (refersTo != null)
                log.AddInfo(null, "`{0}`, in this context, refers to something in global scope.", ident);
            else log.AddInfo(null, "`{0}`, in this context, does not refer to something in global scope.", ident);

            return refersTo;
        }

        public void Visit(NodeStructDeclaration node)
        {
            // TODO(kai): check and update the types
        }

        public void Visit(NodeAssignment node)
        {
            node.Target.Accept(this);
            node.Value.Accept(this);
        }

        public void Visit(NodeBoolLiteral node)
        {
        }

        public void Visit(NodeRealLiteral node)
        {
        }

        public void Visit(NodeStringLiteral node)
        {
        }

        public void Visit(NodeFieldIndex node)
        {
            node.Target.Accept(this);
            // TODO(kai): check and update the type
        }

        public void Visit(NodeExplicitCast node)
        {
            node.Target.Accept(this);
        }

        public void Visit(NodePrefix node)
        {
            node.Target.Accept(this);
        }

        public void Visit(NodeNew node)
        {
        }

        public void Visit(NodeDefer node)
        {
            node.Target.Accept(this);
        }

        public void Visit(NodeWhileUntil node)
        {
        }

        public void Visit(NodeIfUnless node)
        {
        }

        public void Visit(NodeDelete node)
        {
            node.Target.Accept(this);
        }

        public void Visit(NodeBlock node)
        {
            node.Body.ForEach(n => n.Accept(this));
        }

        public void Visit(NodeInfix node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
        }

        public void Visit(NodeInvocation node)
        {
        }

        public void Visit(NodeIdentifier node)
        {
            var ident = node.Identifier;
            var refersTo = GetReferencedSymbol(ident);
        }

        public void Visit(NodeCharLiteral node)
        {
        }

        public void Visit(NodeIntegerLiteral node)
        {
        }

        public void Visit(NodeOperatorAssignment node)
        {
            node.Target.Accept(this);
            node.Value.Accept(this);
        }

        public void Visit(NodeBindingDeclaration node)
        {
            walker.WalkSymbol(node.Symbol);
            // NOTE(kai): type inference/checking happens after symbolic resolution
            node.Value.Accept(this);
        }

        public void Visit(NodeProcedureDeclaration node)
        {
            // FIXME(kai): resolve type

            if (node.Body == null)
                return;

            if (node.IsGlobal)
                walker.StepIntoGlobalScope(node.Symbol);
            else
            {
                // FIXME(kai): step into local scopes
            }

            node.Body.Value.Accept(this);
        }

        public void Visit(NodeLoad node)
        {
        }
    }
}
