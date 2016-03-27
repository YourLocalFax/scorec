namespace ScoreC.Compile.SyntaxTree
{
    abstract class NodeExpression : Node
    {
        /// <summary>
        /// Used to determine if other pieces of code are intending to
        ///  use the result of this expression.
        /// If this is false, this expression will be executed without
        ///  leaving a result, if possible, or will dispose of the
        ///  result in some way when it's finished.
        /// </summary>
        public bool IsResultRequired = false;

        /// <summary>
        /// Used to determine if this is the last piece of code in
        ///  a list of other nodes.
        /// If this is true, the compiler knows this is the last part
        ///  of a set if nodes, which makes knowing where the exit points
        ///  of a procedure are, as well as expression blocks, easy.
        /// </summary>
        public bool InTailPosition = false;

        /// <summary>
        /// Used to determine if this expression is an LValue.
        /// An LValue can be the left-hand side of an expression.
        /// </summary>
        public virtual bool IsLValue => false;

        /// <summary>
        /// The type info of this expression.
        /// If this is known at parse time (certain literals, for example) then it will be set then.
        /// Otherwise this gets set during the type-check phase.
        /// </summary>
        public TypeInfo TypeInfo;
    }
}
