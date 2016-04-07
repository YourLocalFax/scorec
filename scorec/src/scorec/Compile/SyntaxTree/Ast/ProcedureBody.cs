namespace ScoreC.Compile.SyntaxTree
{
    class ProcedureBody
    {
        private NodeExpression value;
        /// <summary>
        /// The value of this body.
        /// If this body was declared without an '=' token, then this MUST
        ///  be a block expression.
        /// Otherwise any expession (including block expressions) is allowed.
        /// </summary>
        public NodeExpression Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (value is NodeBlock)
                    (value as NodeBlock).CreateScope = false;
            }
        }

        public ProcedureBody(NodeExpression value)
        {
            Value = value;
        }
    }
}
