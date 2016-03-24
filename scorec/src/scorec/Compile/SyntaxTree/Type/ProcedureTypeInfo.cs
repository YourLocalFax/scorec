using System.Collections.Generic;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    sealed class ProcedureTypeInfo : TypeInfo
    {
        public sealed class Parameter
        {
            public string Name;
            public TypeInfo Type;

            public Parameter(string name, TypeInfo type)
            {
                Name = name;
                Type = type;
            }

            public override string ToString() =>
                Name == null ? Type.ToString() : string.Format("{0}: {1}", Name, Type.ToString());
        }

        public Token DelimOpenBracket;
        public Token DelimCloseBracket;
        public Token DelimArrow;

        public List<Parameter> Parameters;
        public List<Parameter> Returns;

        public ProcedureTypeInfo(Token delimOpenBracket, Token delimCloseBracket, Token delimArrow, List<Parameter> parameters, List<Parameter> returns)
        {
            DelimOpenBracket = delimOpenBracket;
            DelimCloseBracket = delimCloseBracket;
            DelimArrow = delimArrow;
            Parameters = parameters;
            Returns = returns;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append('(');
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i > 0)
                    buffer.Append(", ");
                buffer.Append(Parameters[i]);
            }
            buffer.Append(')');

            if (DelimArrow != null)
            {
                buffer.Append(" -> ");
                for (int i = 0; i < Returns.Count; i++)
                {
                    if (i > 0)
                        buffer.Append(", ");
                    buffer.Append(Returns[i]);
                }
            }

            return buffer.ToString();
        }
    }
}
