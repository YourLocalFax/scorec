using System.Collections.Generic;
using System.Diagnostics;

namespace ScoreC.Compile.Analysis
{
    class SymbolTableWalker
    {
        public SymbolTable SymbolTable;

        private List<SymbolTable> scopes = new List<SymbolTable>();

        public SymbolTableWalker(SymbolTable symbols)
        {
            SymbolTable = symbols;
            System.Console.WriteLine(SymbolTable);
        }

        public void StepIntoGlobalScope(Symbol global)
        {
            var scope = SymbolTable.FindGlobalScope(global);
#if DEBUG
            Debug.Assert(scope != null);
#endif
            scopes.Clear();
            scopes.Add(scope);
        }

        /// <summary>
        /// `symbol` is passed merely as an error checker.
        /// </summary>
        /// <param name="symbol"></param>
        public void WalkSymbol(Symbol symbol)
        {
        }
    }
}
