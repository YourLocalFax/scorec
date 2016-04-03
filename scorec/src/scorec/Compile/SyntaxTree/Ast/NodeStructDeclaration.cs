using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeStructDeclaration : Node
    {
        private Span start;
        public override Span Start => start;

        public Token TkName;
        public string Name => TkName.Identifier;

        public List<Binding> Fields;

        public StructTypeInfo TypeInfo;

        public NodeStructDeclaration(Span start, Token tkName, List<Binding> fields)
        {
            this.start = start;
            TkName = tkName;
            Fields = fields;

            TypeInfo = new StructTypeInfo();

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                TypeInfo.AddFieldInfo(field.Name, field.DeclaredTypeInfo);
            }
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
