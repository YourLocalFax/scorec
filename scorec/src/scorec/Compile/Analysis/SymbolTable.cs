using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreC.Compile.Analysis
{
    using SyntaxTree;

    [Flags]
    enum SymbolKind
    {
        None = 0,
        Global = 1,
        Local = 2,
        Scope = 4,
        Procedure = 8,
    }

    class Symbol
    {
        public string Name;
        public TypeInfo TypeInfo;
        public SymbolKind Kind;

        public Symbol(string name, TypeInfo typeInfo, SymbolKind kind)
        {
            Name = name;
            TypeInfo = typeInfo;
            Kind = kind;
        }

        public override string ToString() =>
            Name + " : " + (TypeInfo?.ToString() ?? "[unknown]");
    }

    class SymbolTable : Symbol
    {
        private List<Symbol> symbols = new List<Symbol>();

        public IEnumerable<Symbol> DEBUG_SymbolList => symbols;

        /// <summary>
        /// Base for a symbol table
        /// </summary>
        public SymbolTable(string name = null, SymbolKind kind = SymbolKind.Global)
            : base(name, null, kind | SymbolKind.Scope)
        {
        }

        /*
        public Symbol FindMostRecent(string name)
        {
            for (int i = symbols.Count; i >= 0; i--)
                if (symbols[i].Name == name)
                    return symbols[i];
            return null;
        }

        public bool IsDefined(string name)
        {
            // TODO(kai): this will need to change when overloading is a thing
            return FindMostRecent(name) != null;
        }
        */

        public void AddSymbol(string name, TypeInfo typeInfo, SymbolKind kind)
        {
            // FIXME(kai): Log an error when a symbol conflicts with another.
            symbols.Add(new Symbol(name, typeInfo, kind));
        }

        public SymbolTable AddScope(string optName, SymbolKind kind)
        {
            var scope = new SymbolTable(optName, kind);
            symbols.Add(scope);
            return scope;
        }

        public string ToString(int indentations)
        {
            var buffer = new StringBuilder();

            for (int i = 0; i < indentations; i++)
                buffer.Append("  ");

            buffer.Append("/# ").AppendLine(Kind.ToString());

            for (int i = 0; i < indentations; i++)
                buffer.Append("  ");

            if (Name != null)
                buffer.Append(Name).Append(" : ");
            buffer.AppendLine("{");

            foreach (var sym in symbols)
            {
                if (sym is SymbolTable)
                    buffer.AppendLine((sym as SymbolTable).ToString(indentations + 1));
                else
                {
                    var indents = new StringBuilder();
                    for (int i = 0; i < indentations + 1; i++)
                        indents.Append("  ");
                    buffer.Append(indents.ToString());
                    buffer.Append("/# ").AppendLine(sym.Kind.ToString());
                    buffer.Append(indents.ToString());
                    buffer.AppendLine(sym.ToString().Replace("\n", "\n" + indents));
                }
            }
            for (int i = 0; i < indentations; i++)
                buffer.Append("  ");
            buffer.Append("}");

            return buffer.ToString();
        }

        public override string ToString() =>
            ToString(0);
    }
}
