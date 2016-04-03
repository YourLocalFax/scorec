namespace ScoreC.Compile.Analysis
{
    using System.Linq;
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

            node.Body.Value.InTailPosition = true;
            if (node.Returns)
                node.Body.Value.IsResultRequired = true;
            else
            {
                if (node.Body.Value is NodeBlock)
                    (node.Body.Value as NodeBlock).CanBeExpression = false;
            }

            node.Body.Value.Accept(this);
        }

        public void Visit(NodeStructDeclaration node)
        {
            builder.AddSymbol(node.Name, node.TypeInfo, SymbolKind.Struct);
            // TODO(kai): Initializers should be checked
        }

        public void Visit(NodeBindingDeclaration node)
        {
            builder.AddSymbol(node.Binding.Name, node.Binding.DeclaredTypeInfo);
            // TODO(kai): Initializer should be checked
            node.Binding.Value.IsResultRequired = true;
        }

        public void Visit(NodeAssignment node)
        {
            node.Target.IsResultRequired = true;
            node.Value.IsResultRequired = true;

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
            node.Target.IsResultRequired = true;
            node.Target.Accept(this);
        }

        public void Visit(NodeExplicitCast node)
        {
            node.Target.IsResultRequired = true;
            node.Target.Accept(this);
        }

        public void Visit(NodePrefix node)
        {
            node.Target.IsResultRequired = true;
            node.Target.Accept(this);
        }

        public void Visit(NodeNew node)
        {
            // TODO(kai): Arguments should be checked
        }

        public void Visit(NodeDefer node)
        {
            if (node.Target is NodeExpression)
                (node.Target as NodeExpression).IsResultRequired = true;
            node.Target.Accept(this);
        }

        public void Visit(NodeWhileUntil node)
        {
            node.Condition.IsResultRequired = true;
            node.Pass.IsResultRequired = true;
            node.Fail.IsResultRequired = true;

            node.Condition.Accept(this);
            node.Pass.Accept(this);
            node.Fail.Accept(this);
        }

        public void Visit(NodeIfUnless node)
        {
            node.Condition.IsResultRequired = true;
            node.Pass.IsResultRequired = true;
            node.Fail.IsResultRequired = true;

            node.Condition.Accept(this);
            node.Pass.Accept(this);
            node.Fail.Accept(this);
        }

        public void Visit(NodeDelete node)
        {
            node.Target.IsResultRequired = true;
            node.Target.Accept(this);
        }

        public void Visit(NodeBlock node)
        {
            var last = node.Body.Last();
            last.InTailPosition = node.InTailPosition;
            if (last is NodeExpression)
                (last as NodeExpression).IsResultRequired = node.IsResultRequired;

            builder.PushScope(null);
            node.Body.ForEach(n => n.Accept(this));
            builder.PopScope();
        }

        public void Visit(NodeInfix node)
        {
            node.Left.IsResultRequired = true;
            node.Right.IsResultRequired = true;

            node.Left.Accept(this);
            node.Right.Accept(this);
        }

        public void Visit(NodeInvocation node)
        {
            node.Target.IsResultRequired = true;

            node.Target.Accept(this);
            node.ArgumentList.ForEach(arg => { arg.IsResultRequired = true; arg.Accept(this); });
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
            node.Target.IsResultRequired = true;
            node.Value.IsResultRequired = true;
            
            node.Target.Accept(this);
            node.Value.Accept(this);
        }

        public void Visit(NodeLoad node)
        {
        }
    }
}
