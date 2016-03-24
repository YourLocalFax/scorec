namespace ScoreC.Compile.SyntaxTree
{
    class ProcedureBody
    {
        /// <summary>
        /// The value of this body.
        /// If this body was declared without an '=' token, then this MUST
        ///  be a block expression.
        /// Otherwise any expession (including block expressions) is allowed.
        /// </summary>
        public NodeExpression Value;

        public ProcedureBody(NodeExpression value)
        {
            Value = value;
        }
    }
}
