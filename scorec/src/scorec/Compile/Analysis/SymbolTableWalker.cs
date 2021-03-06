﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ScoreC.Compile.Analysis
{
    class SymbolTableWalker
    {
        private class Scout
        {
            public SymbolTable Scope;
            public int Steps;

            public bool IsFinished => Steps >= Scope.Symbols.Count;

            public Scout(SymbolTable scope)
            {
                Scope = scope;
            }

            public void WalkSymbol(Symbol symbol)
            {
#if DEBUG
                Debug.Assert(!IsFinished, "no symbols to walk, " + symbol.Name);
                var current = Scope.Symbols[Steps];
                Debug.Assert(current == symbol, "failed to walk a symbol, " + symbol.Name);
#endif
                Steps++;
            }

            public Symbol GetLatestSymbolByName(string ident)
            {
                for (int i = Steps - 1; i >= 0; i--)
                {
                    var symbol = Scope.Symbols[i];
                    if (symbol.Name == ident)
                        return symbol;
                }
                return null;
            }
        }

        public SymbolTable SymbolTable;

        private List<Scout> scouts = new List<Scout>();

        public SymbolTableWalker(SymbolTable symbols)
        {
            SymbolTable = symbols;
        }

        public void StepIntoGlobalScope(Symbol global)
        {
            var scope = SymbolTable.FindGlobalScope(global);
#if DEBUG
            Debug.Assert(scope != null, "failed to step into a global scope");
#endif
            scouts.Clear();
            scouts.Add(new Scout(scope));
        }

        public Symbol FindNearestSymbolByName(string ident)
        {
            for (int i = scouts.Count - 1; i >= 0; i--)
            {
                var symbol = scouts[i].GetLatestSymbolByName(ident);
                if (symbol != null)
                    return symbol;
            }
            return SymbolTable.FindGlobal(ident);
        }

        public void StepIntoScope()
        {
            var scout = scouts.Last();
            var symTab = scout.Scope.Symbols[scout.Steps];
#if DEBUG
            Debug.Assert(symTab is SymbolTable);
#endif
            scout.Steps++;
            scouts.Add(new Scout(symTab as SymbolTable));
        }

        public void StepOutOfScope()
        {
#if DEBUG
            Debug.Assert(scouts.Last().IsFinished, "Failed to step out of a scope");
#endif
            scouts.RemoveAt(scouts.Count - 1);
        }

        /// <summary>
        /// `symbol` is passed merely as an error checker.
        /// </summary>
        /// <param name="symbol"></param>
        public void WalkSymbol(Symbol symbol)
        {
            var scout = scouts.Last();
            scout.WalkSymbol(symbol);
        }
    }
}
