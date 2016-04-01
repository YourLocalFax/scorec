namespace ScoreC.Compile.SyntaxTree
{
    interface IAstVisitor
    {
        void Visit(NodeLoad node);

        void Visit(NodeProcedureDeclaration node);
        void Visit(NodeStructDeclaration node);
        void Visit(NodeBindingDeclaration node);
        void Visit(NodeAssignment node);
        void Visit(NodeOperatorAssignment node);

        #region Literals
        void Visit(NodeBoolLiteral node);
        void Visit(NodeIntegerLiteral node);
        void Visit(NodeRealLiteral node);
        void Visit(NodeCharLiteral node);
        void Visit(NodeStringLiteral node);
        // TODO(kai): other literals
        #endregion

        void Visit(NodeIdentifier node);
        void Visit(NodeFieldIndex node);
        void Visit(NodeInvocation node);
        void Visit(NodeExplicitCast node);
        //void Visit(NodeAutoCast node);
        void Visit(NodeInfix node);
        void Visit(NodePrefix node);
        void Visit(NodeBlock node);
        void Visit(NodeNew node);
        void Visit(NodeDelete node);
        void Visit(NodeDefer node);

        void Visit(NodeIfUnless node);
        void Visit(NodeWhileUntil node);
    }
}
