#define DEBUG_REFERS_TO

namespace ScoreC.Compile.Analysis
{
    using System.Diagnostics;
    using System.Linq;
    using Logging;
    using Source;
    using SyntaxTree;

    class SymbolicResolver : IAstVisitor
    {
        public static void Resolve(Project project)
        {
            var walker = new SymbolTableWalker(project.SymbolTable);
            foreach (var file in project.SourceMaps)
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
            return walker.FindNearestSymbolByName(ident);
        }

        public void Visit(NodeStructDeclaration node)
        {
            if (!node.IsGlobal)
                walker.WalkSymbol(node.Symbol);
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

            var targetType = node.Target.TypeInfo;
            Debug.Assert(targetType != null, "Expected target to have a type, none found!");

            if (targetType is StructTypeInfo)
            {
                var structType = targetType as StructTypeInfo;

                StructTypeInfo.FieldInfo fieldInfo;
                if (structType.TryGetFieldInfo(node.FieldName, out fieldInfo))
                {
                    var fieldTypeInfo = fieldInfo.TypeInfo;
                    Debug.Assert(fieldTypeInfo != null, "Expected struct field `" + node.FieldName + "` to have a type, none found!");
                    node.TypeInfo = fieldTypeInfo;

#if DEBUG_REFERS_TO
                    var start = node.Target.Start;
                    var end = node.TkFieldNameIdentifier.Span;
                    System.Console.WriteLine("`" + file.GetSourceAtSpan(new Span(file, start.Line, start.Column, end.EndLine, end.EndColumn)) + "` has type `" + fieldTypeInfo + "`");
#endif
                }
                else log.AddError(node.TkFieldNameIdentifier.Span, "Struct `" + structType + "` doesn't contain a field named `" + node.FieldName + "`.");
            }
            else log.AddError(node.TkFieldNameIdentifier.Span, "Cannot dereference type " + targetType);

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
            node.Target.Accept(this);

            foreach (var arg in node.ArgumentList)
                arg.Accept(this);
        }

        public void Visit(NodeIdentifier node)
        {
            var ident = node.Identifier;
            node.ReferencedSymbol = GetReferencedSymbol(ident);
            node.TypeInfo = node.ReferencedSymbol?.TypeInfo;

#if DEBUG_REFERS_TO
            System.Console.WriteLine("`" + ident + "` refers to symbol `" + node.ReferencedSymbol + "`");
            System.Console.WriteLine("`" + ident + "` is of type `" + node.TypeInfo + "`");
#endif
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

        private TypeInfo ResolveType(Span start, TypeInfo typeInfo, out Symbol symbol)
        {
            symbol = null;

            if (typeInfo is QualifiedTypeInfo)
            {
                var qualifiedTypeInfo = typeInfo as QualifiedTypeInfo;
                var path = qualifiedTypeInfo.Path;

                Debug.Assert(path.Count > 0, "Qualified type infos must contain at least one path node!");

                var root = path[0];
                Debug.Assert(root != null, "Need root!");

                // FIXME(kai): When we do namespacing, there can be multiple nodes to the path!!!
                if (path.Count > 1)
                    log.AddError(root.Span, "Currently, qualified type paths must only be a single identifier.");
                else if (!root.IsIdentifier)
                    log.AddError(root.Span, "Qualified type path must begin with an identifier.");
                else
                {
                    var refersTo = GetReferencedSymbol(root.Identifier);
                    if (refersTo == null)
                        log.AddError(root.Span, "Failed to determine what `{0}` refers to.", root.Identifier);
                    else
                    {
                        // FIXME(kai): Other things can be types, so include those when they exist!
                        if (refersTo.IsStruct)
                        {
                            symbol = refersTo;
#if DEBUG_REFERS_TO
                            System.Console.WriteLine("`" + qualifiedTypeInfo + "` refers to type `" + symbol + "`");
#endif
                            return symbol.TypeInfo;
                        }
                        else log.AddError(root.Span, "`{0}` does not refer to a type!", root.Identifier);
                    }
                }
            }
            else if (typeInfo is BuiltinTypeInfo)
                return typeInfo;

            log.AddError(start, "Failed to resolve type.");
            return null;
        }

        public void Visit(NodeBindingDeclaration node)
        {
            var symbol = node.Symbol;
            Debug.Assert(symbol != null, "Binding declaration requires a symbol, none found!");

            if (!node.Binding.ShouldBeTypeInferred)
            {
                var typeInfo = node.Binding.DeclaredTypeInfo;

                var type = ResolveType(node.Binding.TypeStart, typeInfo, out node.Binding.TypeSymbol);
                node.Symbol.TypeInfo = node.Binding.TypeSymbol.TypeInfo = type;
            }

            // NOTE(kai): type checking happens after symbolic resolution
            // FIXME(kai): type inference should happen here
            node.Value?.Accept(this);

            if (!node.IsGlobal)
                walker.WalkSymbol(node.Symbol);
        }

        public void Visit(NodeProcedureDeclaration node)
        {
            // FIXME(kai): resolve type

            if (!node.IsGlobal)
                walker.WalkSymbol(node.Symbol);

            if (node.Body == null)
                return;

            if (node.IsGlobal)
                walker.StepIntoGlobalScope(node.Symbol);
            else walker.StepIntoScope();
            node.Body.Value.Accept(this);
            walker.StepOutOfScope();
        }

        public void Visit(NodeLoad node)
        {
        }
    }
}
