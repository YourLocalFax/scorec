using System.Collections.Generic;

namespace ScoreC.Compile.SyntaxTree
{
    using Source;

    class NodeInvocation : NodeExpression
    {
        public override Span Start => Target.Start;

        public NodeExpression Target;
        public List<NodeExpression> ArgumentList;

        public NodeInvocation(NodeExpression target, List<NodeExpression> argumentList)
        {
            Target = target;
            ArgumentList = argumentList;
        }
        
        public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    }
}
