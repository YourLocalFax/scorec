using System;
using System.Collections.Generic;
using System.Text;

namespace ScoreC.Compile.Analysis
{
    using Source;
    using SyntaxTree;

    [Flags]
    enum SymbolKind
    {
        None = 0,
        Local = 1,
        Scope = 2,
        Struct = 4,
        Procedure = 5,
    }

    class Symbol
    {
        public Span Location;
        public string Name;
        public TypeInfo TypeInfo;
        public SymbolKind Kind;

        public bool IsGlobal => (Kind & SymbolKind.Local) == 0;

        public bool IsStruct => (Kind & SymbolKind.Struct) != 0;
        public bool IsProcedure => (Kind & SymbolKind.Procedure) != 0;
        public bool IsScope => (Kind & SymbolKind.Scope) != 0;

        public Symbol(Span location, string name, TypeInfo typeInfo, SymbolKind kind)
        {
            Location = location;
            Name = name;
            TypeInfo = typeInfo;
            Kind = kind;
        }

        public override string ToString() =>
            Name + " : " + (TypeInfo?.ToString() ?? "[unknown]");
    }

    class SymbolTable : Symbol
    {
        public List<Symbol> Symbols = new List<Symbol>();

        /// <summary>
        /// Base for a symbol table
        /// </summary>
        public SymbolTable(string name = null, SymbolKind kind = SymbolKind.None)
            : base(null, name, null, kind | SymbolKind.Scope)
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

        public Symbol AddSymbol(Span location, string name, TypeInfo typeInfo, SymbolKind kind)
        {
            // FIXME(kai): Log an error when a symbol conflicts with another.
            var symbol = new Symbol(location, name, typeInfo, kind);
            Symbols.Add(symbol);
            return symbol;
        }

        public Symbol FindGlobal(string name) =>
            Symbols.Find(symbol => symbol.Name == name);

        public SymbolTable FindGlobalScope(Symbol global)
        {
            for (int i = 0; i < Symbols.Count; i++)
            {
                var symbol = Symbols[i];
                if (symbol == global && i < Symbols.Count - 1)
                    return Symbols[i + 1] as SymbolTable;
            }
            return null;
        }

        public SymbolTable AddScope(string optName, SymbolKind kind)
        {
            var scope = new SymbolTable(optName, kind);
            Symbols.Add(scope);
            return scope;
        }

        public string ToString(int indentations)
        {
            var buffer = new StringBuilder();

            for (int i = 0; i < indentations; i++)
                buffer.Append("  ");

            buffer.Append("/# ").AppendLine(Kind.ToString().Replace(",", ""));

            for (int i = 0; i < indentations; i++)
                buffer.Append("  ");

            if (Name != null)
                buffer.Append(Name).Append(" : ");
            buffer.AppendLine("{");

            foreach (var sym in Symbols)
            {
                if (sym is SymbolTable)
                    buffer.AppendLine((sym as SymbolTable).ToString(indentations + 1));
                else
                {
                    var indents = new StringBuilder();
                    for (int i = 0; i < indentations + 1; i++)
                        indents.Append("  ");

                    buffer.Append(indents.ToString());
                    buffer.Append("/# ").AppendLine(sym.Kind.ToString().Replace(",", ""));

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
