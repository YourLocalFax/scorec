namespace ScoreC.Compile.Analysis
{
    using Source;
    using SyntaxTree;

    class SemanticAnalyzer : IAstVisitor
    {
        public static void Analyze(SourceMap map, SymbolTableBuilder builder)
        {
            var analyzer = new SemanticAnalyzer(builder);
            map.Ast.Accept(analyzer);
        }

        private SymbolTableBuilder builder;

        private SemanticAnalyzer(SymbolTableBuilder builder)
        {
            this.builder = builder;
        }

        public void Visit(NodeProcedureDeclaration node)
        {
            builder.AddSymbol(node.Name, node.TypeInfo, SymbolKind.Procedure);
            node.Body.Value.Accept(this);
        }

        public void Visit(NodeStructDeclaration node)
        {
            builder.AddSymbol(node.Name, node.TypeInfo);
            // TODO(kai): Initializers should be checked
        }

        public void Visit(NodeBindingDeclaration node)
        {
            builder.AddSymbol(node.Binding.Name, node.Binding.DeclaredTypeInfo);
            // TODO(kai): Initializer should be checked
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
            // TODO(kai): Arguments should be checked
        }

        public void Visit(NodeDefer node)
        {
            node.Target.Accept(this);
        }

        public void Visit(NodeWhileUntil node)
        {
            node.Condition.Accept(this);
            node.Pass.Accept(this);
            node.Fail.Accept(this);
        }

        public void Visit(NodeIfUnless node)
        {
            node.Condition.Accept(this);
            node.Pass.Accept(this);
            node.Fail.Accept(this);
        }

        public void Visit(NodeDelete node)
        {
            node.Target.Accept(this);
        }

        public void Visit(NodeBlock node)
        {
            builder.PushScope(null);
            node.Body.ForEach(n => n.Accept(this));
            builder.PopScope();
        }

        public void Visit(NodeInfix node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
        }

        public void Visit(NodeInvocation node)
        {
            node.Target.Accept(this);
            node.ArgumentList.ForEach(arg => arg.Accept(this));
        }

        public void Visit(NodeIdentifier node)
        {
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

        public void Visit(NodeLoad node)
        {
        }
    }
}
