using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Analysis;
    using Source;

    class NodeStructDeclaration : Node
    {
        private Span start;
        public override Span Start => start;

        public Token TkName;
        public string Name => TkName.Identifier;

        public List<Binding> Parameters;
        public List<Binding> Fields;

        public StructTypeInfo TypeInfo;

        public bool IsGlobal;
        public Symbol Symbol;

        public NodeStructDeclaration(Span start, Token tkName, List<Binding> parameters, List<Binding> fields)
        {
            this.start = start;
            TkName = tkName;
            Parameters = parameters;
            Fields = fields;

            TypeInfo = new StructTypeInfo();

            foreach (var param in parameters)
                TypeInfo.AddParameterInfo(param.Name, param.DeclaredTypeInfo);
            foreach (var field in fields)
                TypeInfo.AddFieldInfo(field.Name, field.DeclaredTypeInfo);
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
