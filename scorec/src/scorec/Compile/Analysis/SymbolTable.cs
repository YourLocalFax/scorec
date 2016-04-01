using System.Collections.Generic;
using System.Diagnostics;

namespace ScoreC.Compile.Analysis
{
    using SyntaxTree;

    class Symbol
    {
        public string Name;
        public TypeInfo TypeInfo;

        public Symbol(string name, TypeInfo typeInfo)
        {
            Name = name;
            TypeInfo = typeInfo;
        }

        public override string ToString() =>
            Name + " : " + TypeInfo.ToString();
    }

    class SymbolTable
    {
        private Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        public bool IsDefined(string name)
        {
            // TODO(kai): this will need to change when overloading is a thing
            return symbols.ContainsKey(name);
        }

        public void AddSymbol(string name, TypeInfo typeInfo)
        {
#if DEBUG
            Debug.Assert(!IsDefined(name));
#endif
            symbols[name] = new Symbol(name, typeInfo);
        }
    }
}
