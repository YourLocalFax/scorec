﻿namespace ScoreC.Compile.SyntaxTree
{
    interface IAstVisitor
    {
        void Visit(NodeProcedureDeclaration node);
        void Visit(NodeBlock node);

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
    }
}
