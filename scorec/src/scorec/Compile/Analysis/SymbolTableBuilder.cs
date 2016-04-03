using System.Collections.Generic;

namespace ScoreC.Compile.Analysis
{
    using SyntaxTree;

    class SymbolTableBuilder
    {
        public SymbolTable SymbolTable;

        private Stack<SymbolTable> scopes = new Stack<SymbolTable>();

        public SymbolTableBuilder()
        {
            SymbolTable = new SymbolTable();
            scopes.Push(SymbolTable);
        }

        public void AddSymbol(string name, TypeInfo typeInfo, SymbolKind kind = SymbolKind.None) =>
            scopes.Peek().AddSymbol(name, typeInfo, kind | (scopes.Count == 1 ? SymbolKind.Global : SymbolKind.Local));

        public void PushScope(string optName, SymbolKind kind = SymbolKind.None)
        {
            var newScope = scopes.Peek().AddScope(optName, kind | SymbolKind.Local);
            scopes.Push(newScope);
        }

        public void PopScope()
        {
            scopes.Pop();
        }
    }
}
