using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeInvocation : NodeExpression
    {
        public override Span Start => Target.Start;

        public Token TkOpenBracket;
        public Token TkCloseBracket;

        public NodeExpression Target;
        public List<NodeExpression> ArgumentList;

        public NodeInvocation(Token tkOpenBracket, Token tkCloseBracket, NodeExpression target, List<NodeExpression> argumentList)
        {
            TkOpenBracket = tkOpenBracket;
            TkCloseBracket = tkCloseBracket;
            Target = target;
            ArgumentList = argumentList;
        }
        
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
