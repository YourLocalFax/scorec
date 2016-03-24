namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class ProcedureBody
    {
        /// <summary>
        /// The optional '=' token that can precede the body.
        /// If this exists, the procedure is expected to return a value.
        /// If it doesn't, the compiler will be unhappy.
        /// </summary>
        public Token OpEqual;

        /// <summary>
        /// The value of this body.
        /// If this body was declared without an '=' token, then this MUST
        ///  be a block expression.
        /// Otherwise any expession (including block expressions) is allowed.
        /// </summary>
        public NodeExpression Value;

        public ProcedureBody(Token opEqual, NodeExpression value)
        {
            OpEqual = opEqual;
            Value = value;
        }

        public ProcedureBody(NodeExpression value)
        {
            Value = value;
        }
    }
}
