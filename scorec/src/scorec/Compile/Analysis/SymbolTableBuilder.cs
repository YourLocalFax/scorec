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

        public void AddSymbol(string name, TypeInfo typeInfo) =>
            scopes.Peek().AddSymbol(name, typeInfo);

        public void PushScope(string optName = null)
        {
            var newScope = SymbolTable.AddScope(optName);
            scopes.Push(newScope);
        }

        public void PopScope()
        {
            scopes.Pop();
        }
    }
}
