namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeProcedureDeclaration : Node
    {
        public override Span Start => KwProc.Span;

        /// <summary>
        /// The 'proc' keyword used to declare this procedure.
        /// </summary>
        public Token KwProc;

        /// <summary>
        /// The identifier or operator name that this procedure was declared with.
        /// </summary>
        public Token TkName;
        /// <summary>
        /// The string representation of the name this procedure was declared with.
        /// </summary>
        public string Name => TkName.Image;

        /// <summary>
        /// The type of this procedure.
        /// This contains parameter and return information.
        /// </summary>
        public ProcedureTypeInfo TypeInfo;

        // TODO(kai): probably provide a way to iterate over parameters & return types here rather than going through the type declaration all the time.

        /// <summary>
        /// The body of this procedure.
        /// </summary>
        public ProcedureBody Body;

        public NodeProcedureDeclaration(Token kwProc, Token tkName, ProcedureTypeInfo typeInfo, ProcedureBody body)
        {
            KwProc = kwProc;
            TkName = tkName;
            TypeInfo = typeInfo;
            Body = body;
        }

        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
