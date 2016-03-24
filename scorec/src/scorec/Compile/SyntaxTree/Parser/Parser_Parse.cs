using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    using Logging;
    using Source;
    using System.Globalization;

    sealed partial class Parser
    {
        private Ast GetAst(Log log, SourceMap map)
        {
#if DEBUG
            Debug.Assert(log != null, "Log must not be null!");
            Debug.Assert(map != null, "Map must not be null!");
            Debug.Assert(map.Tokens != null, "Can't parse a map that hasn't been lexed!");
#endif
            // initialize data
            Log = log;
            Map = map;
            tokensCached = map.Tokens;

            // get the AST
            var ast = new Ast(map);

            bool handleFailureMessage;
            while (!IsEndOfSource)
            {
                var start = Current;
                var node = GetNode(out handleFailureMessage);

                if (node != null)
                    ast.AddNode(node);
                else if (handleFailureMessage)
                {
                    var message = Message.NodeParseFailed(start);
                    Log.AddError(message);
                    Advance(); // Skip this one plz.
                }
            }

            // get rid of data, we're done here.
            Map = null;
            Log = null;

            return ast;
        }

        /// <summary>
        /// First attempts to parse statements that can occur at top level.
        /// If that fails, attempts to parse statements that cannot
        ///  occur at top level and expressions.
        /// If it can't 
        /// </summary>
        /// <param name="handleFailureMessage"></param>
        /// <returns></returns>
        private Node GetNode(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(!IsEndOfSource, "No Tokens!! Called at end of source!");
#endif

            switch (Current.Kind)
            {
            case TokenKind.Keyword:
                switch (Current.Keyword)
                {
                // This control flow is sad, plz help
                case Keyword.Extern:
                    bool is_extern = true;
                    Advance();
                    goto in_keyword_proc;

                case Keyword.Proc:
                    is_extern = false;
                in_keyword_proc:
                    return ParseProcedureDeclaration(is_extern, out handleFailureMessage);

                default: goto in_default;
                }

            default:
            in_default:
                var expr = ParseExpression(out handleFailureMessage);
                // TODO(kai): Assignment statement here plz
                return expr;
            }

            // we're failures, oh well...
            return null;
        }

        #region Expressions
        private NodeExpression ParseExpression(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
            // TODO(kai): Infix expressions here plz
            return ParsePrimaryExpression(out handleFailureMessage);
        }

        private NodeExpression ParsePrimaryExpression(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
            if (IsEndOfSource)
                return null;

            NodeExpression result = null;

            if (Check(Keyword.True) || Check(Keyword.False))
            {
                var tkBoolLiteral = Current;
                Advance();
                result = new NodeBoolLiteral(tkBoolLiteral);
            }
            else
            {
                switch (Current.Kind)
                {
                case TokenKind.Integer:
                    var tkIntegerLiteral = Current;
                    Advance();
                    result = new NodeIntegerLiteral(tkIntegerLiteral);
                    break;
                case TokenKind.Real:
                    var tkRealLiteral = Current;
                    Advance();
                    result = new NodeRealLiteral(tkRealLiteral);
                    break;
                case TokenKind.Char:
                    var tkCharLiteral = Current;
                    Advance();
                    result = new NodeCharLiteral(tkCharLiteral);
                    break;
                case TokenKind.String:
                    var tkStringLiteral = Current;
                    Advance();
                    result = new NodeStringLiteral(tkStringLiteral);
                    break;
                case TokenKind.Identifier:
                    var tkIdentifier = Current;
                    Advance();
                    result = new NodeIdentifier(tkIdentifier);
                    break;
                case TokenKind.OpenCurlyBracket:
                    var tkOpen = Current;
                    Advance(); // `{`

                    // var success = true;
                    var body = new List<Node>();
                    while (!IsEndOfSource && !Check(TokenKind.CloseCurlyBracket))
                    {
                        var node = GetNode(out handleFailureMessage);
                        if (node != null)
                            body.Add(node);
                        else Advance();
                        // else success = false;
                    }

                    if (!Check(TokenKind.CloseCurlyBracket))
                    {
                        if (IsEndOfSource)
                        {
                            var message = Message.UnexpectedEndOfSource(Previous.Span,
                                "Expected `}` to close block, found end of source.");
                            Log.AddError(message);
                        }
                        else
                        {
                            var message = Message.UnexpectedToken(Current.Span,
                                Current.Image, "`}`", "to close block expression");
                            Log.AddError(message);
                        }
                        return result;
                    }

                    var tkClose = Current;
                    Advance(); // `}`

                    result = new NodeBlock(tkOpen, tkClose, body);
                    break;
                default:
                    {
                        handleFailureMessage = false;
                        var message = Message.ExpressionParseFailed(Current);
                        Log.AddError(message);
                        //Advance();
                        return null;
                    }
                }
            }

            if (result == null)
                return null;

            ParseOptionalPrimarySuffix(ref result, out handleFailureMessage);
            return result;
        }

        private List<NodeExpression> ParseCommaSeparatedList(bool allowTrailingComma, TokenKind closeDelimiter, out Token tkCloseDelimiter, out bool handleFailureMessage)
        {
            tkCloseDelimiter = null;
            handleFailureMessage = true;

            var closeDelimiterImage = "???";
            switch (closeDelimiter)
            {
            case TokenKind.CloseBracket:
                closeDelimiterImage = ")"; break;
            case TokenKind.CloseSquareBracket:
                closeDelimiterImage = "]"; break;
            // case TokenKind.CloseCurlyBracket: closeImage = "}"; break;
            default: break;
            }

            var result = new List<NodeExpression>();

            if (Check(closeDelimiter))
            {
                Advance();
                return result;
            }

            while (!IsEndOfSource)
            {
                var cur = Current;
                var expr = ParseExpression(out handleFailureMessage);
                if (expr != null)
                    result.Add(expr);
                else break;

                if (Check(TokenKind.CloseBracket))
                    break;

                if (Check(TokenKind.Comma))
                {
                    Advance(); // `,`

                    if (Check(closeDelimiter))
                    {
                        if (allowTrailingComma)
                        {
                            Advance(); // closeDelimiter
                            return result;
                        }

                        handleFailureMessage = false;
                        var message = Message.UnexpectedToken(Previous.Span,
                            string.Format("Unexpected `,` found before closing delimiter `{0}`.", closeDelimiterImage));
                        Log.AddError(message);
                        return null;
                    }
                    // This is implicit, but keep it here anyway for clarity.
                    else continue;
                }
                else
                {
                    if (Check(closeDelimiter))
                    {
                        Advance(); // closeDelimiter
                        return result;
                    }
                    else
                    {
                        if (IsEndOfSource)
                        {
                            var message = Message.UnexpectedEndOfSource(Previous.Span,
                                string.Format("Expected `{0}`, found end of source.", closeDelimiterImage));
                            Log.AddError(message);
                        }
                        else
                        {
                            var message = Message.UnexpectedToken(Current.Span,
                                Current.Image, "`)`");
                            Log.AddError(message);
                        }
                        return null;
                    }
                }
            }

            if (!Expect(closeDelimiter))
            {
                handleFailureMessage = false;
                if (IsEndOfSource)
                {
                    var message = Message.UnexpectedEndOfSource(Previous.Span,
                        string.Format("Expected `{0}` to close argument list, found end of source.", closeDelimiterImage));
                    Log.AddError(message);
                }
                else
                {
                    var message = Message.UnexpectedToken(Current.Span,
                        Current.Image, "`)`", "to close argument list");
                    Log.AddError(message);
                }
                return null;
            }
            else tkCloseDelimiter = Previous;

            return result;
        }

        /// <summary>
        /// Handles parsing suffixes on primary expressions.
        /// This includes `a as b`, `a(b)`, `a.b`, `a[b]`.
        /// 
        /// If this fails to detect a suffix case, result is left unchanged.
        /// If this fails while parsing a suffix, result is set to null.
        /// The suffix parsing may continue even if it fails in an attempt
        ///  to make parsing better later.
        /// Otherwise if this succeeds in parsing a suffix, result is updated.
        /// </summary>
        private void ParseOptionalPrimarySuffix(ref NodeExpression result, out bool handleFailureMessage)
        {
            handleFailureMessage = true;
            if (IsEndOfSource)
                return;

            if (Check(Keyword.As))
            {
                var tkAs = Current;
                Advance(); // `as`
                var typeInfo = ParseTypeInfo(out handleFailureMessage);
                if (typeInfo == null)
                {
                    result = null;
                    return;
                }
                //result = new NodeExplicitCast(result, typeInfo);
                return;
            }

            switch (Current.Kind)
            {
            case TokenKind.OpenBracket:
                {
                    var tkOpenBracket = Current;
                    Advance();

                    Token tkCloseBracket;
                    var argumentList = ParseCommaSeparatedList(false, TokenKind.CloseBracket, out tkCloseBracket, out handleFailureMessage);

                    if (argumentList == null)
                        result = null;
                    else result = new NodeInvocation(tkOpenBracket, tkCloseBracket, result, argumentList);
                    return;
                }
            case TokenKind.Dot:
                {
                    var tkDot = Current;
                    // TODO(kai): .type
                    Advance(); //  `.`
                    var tkIdent = IsEndOfSource ? null : Current;
                    if (Check(TokenKind.Identifier))
                        result = new NodeFieldIndex(tkDot, result, tkIdent);
                    else
                    {
                        handleFailureMessage = false;
                        var message = Message.UnexpectedToken(Current.Span, Current.Image, "Identifier");
                        Log.AddError(message);
                        result = null;
                    }
                    return;
                }
            }
        }
        #endregion

        private TypeInfo ParseTypeInfo(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(!IsEndOfSource, "you're at the end of the source, buddy, stahp.");
#endif

            switch (Current.Kind)
            {
            case TokenKind.Identifier:
                {
                    // URGENT(kai): Handle qualified typenames. This might include going through builtin types, so consider that!
                    if (BuiltinTypeInfo.IsValid(Current.Identifier))
                    {
                        Advance();
                        return BuiltinTypeInfo.Get(Previous.Identifier);
                    }
                    handleFailureMessage = false;
                    var message = Message.TypeParseFailed(Current);
                    Log.AddError(message);
                    return null;
                }
            case TokenKind.Operator:
                if (Check(OperatorKind.Star))
                {
                    var tkStar = Current;
                    Advance();
                    var type = ParseTypeInfo(out handleFailureMessage);
                    if (type == null)
                        return null;
                    return new PointerTypeInfo(tkStar, type);
                }
                return null;
            #region case Tuple or Procedure Type:
            case TokenKind.OpenBracket:
                {
                    var tkOpenBracket = Current;
                    Advance(); // `(`

                    var names = new List<Token>();
                    var types = new List<TypeInfo>();

                    while (!Check(TokenKind.CloseBracket))
                    {
                        Token tkName;
                        if (Check(TokenKind.Identifier) && CheckNext(OperatorKind.Colon))
                        {
                            tkName = Current;
                            Advance(2);
                        }
                        // No name, we'll check for consistency later.
                        else tkName = null;
                        names.Add(tkName);

                        var type = ParseTypeInfo(out handleFailureMessage);
                        if (type == null)
                        {
                            types.Add(null);
                            if (Check(TokenKind.Comma))
                            {
                                var message = Message.MissingType(Current.Span,
                                    "Expected a type in tuple or procedure type, found a comma.");
                                Log.AddError(message);
                                // NOTE(kai): continue on to parse out the comma
                            }
                            else if (CheckNext(TokenKind.Comma))
                            {
                                var message = Message.MissingType(Current.Span,
                                    string.Format("Expected a type in tuple or procedure type, found a `{0}`.", Current.Image));
                                Log.AddError(message);
                                Advance();
                                // NOTE(kai): continue on to parse out the comma
                            }
                            else
                            {
                                handleFailureMessage = false;
                                var message = Message.TypeParseFailed(Current);
                                Log.AddError(message);
                                return null;
                            }
                        }
                        types.Add(type);

                        // If we hit a `)` then we can stop looping
                        if (Check(TokenKind.CloseBracket))
                            break;
                        // Otherwise we expect to keep going, need a comma.
                        else if (!Expect(TokenKind.Comma))
                        {
                            // TODO(kai): Check options here for better recovery/errors.
                            var message = Message.TypeParseFailed(IsEndOfSource ? Current : tkOpenBracket);
                            Log.AddError(message);
                            return null;
                        }
                    }

                    var tkCloseBracket = Current;
                    if (!Expect(TokenKind.CloseBracket))
                    {
                        var message = Message.TypeParseFailed(tkOpenBracket.Span,
                            "Missing `(` to match opening `)` in tuple or procedure type.");
                        Log.AddError(message);
                        return null;
                    }

                    // FIXME(kai): check that the type either always or never defines names.
                    var parameters = new List<ProcedureTypeInfo.Parameter>();
                    var count = types.Count;
                    for (int i = 0; i < count; i++)
                        parameters.Add(new ProcedureTypeInfo.Parameter(names[i]?.Identifier ?? null, types[i]));

                    // Is this actually a procedure type?
                    if (Check(OperatorKind.MinusGreater))
                    {
                        var tkArrow = Current;
                        Advance(); // `->`
                        // TODO(kai): eventually named returns will be a thing, check those here.
                        var returnType = ParseTypeInfo(out handleFailureMessage);
                        if (returnType == null)
                        {
                            if (handleFailureMessage)
                            {
                                handleFailureMessage = false;
                                var message = Message.MissingType(tkArrow.Span,
                                    "Procedure type is missing a return type.");
                                Log.AddError(message);
                            }
                            return null;
                        }

                        // TODO(kai): support multiple return values

                        var returns = new List<ProcedureTypeInfo.Parameter>();
                        returns.Add(new ProcedureTypeInfo.Parameter(null, returnType));

                        return new ProcedureTypeInfo(tkOpenBracket, tkCloseBracket, tkArrow, parameters, returns);
                    }
                    else
                    {
                        // FIXME(kai): when we implement tuples fix this up.
                        var returns = new List<ProcedureTypeInfo.Parameter>();
                        returns.Add(new ProcedureTypeInfo.Parameter(null, BuiltinTypeInfo.Get(BuiltinType.VOID)));

                        return new ProcedureTypeInfo(tkOpenBracket, tkCloseBracket, null, parameters, returns);
                    }
                }
                #endregion
            }

            return null;
        }

        #region Procedures
        /// <summary>
        /// Special case of ParseTypeInfo, guarantees that the type is a procedure type (or it fails, heh).
        /// </summary>
        /// <param name="handleFailureMessage"></param>
        /// <returns></returns>
        private ProcedureTypeInfo ParseProcedureTypeInfo(out bool handleFailureMessage)
        {
            var type = ParseTypeInfo(out handleFailureMessage);
            if (type == null || !(type is ProcedureTypeInfo))
            {
                handleFailureMessage = true;
                return null;
            }
            return type as ProcedureTypeInfo;
        }

        private NodeProcedureDeclaration ParseProcedureDeclaration(bool isExtern, out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(Check(Keyword.Proc), "Expected `proc` to start procedure declaration.");
#endif

            var kwProc = Current;
            Advance(); // `proc`

            // Check for the procedure's name
            Token tkName;
            // we need a name or an operator
            if (Check(TokenKind.Identifier) || Check(TokenKind.Operator))
            {
                // We have a valid name! Take it c:<
                tkName = Current;
                Advance();
            }
            else
            {
                handleFailureMessage = false;
                // Well, this is an issue... Figure out the best way to tell the SHITTY programmer.
                tkName = null;

                string description;
                // If we're missing a name but there's an open bracket ( then that's easy.
                // In this case we assume that the name was either forgotten or removed for some reason.
                if (Check(TokenKind.OpenBracket))
                    description = "Procedure declaration is missing a name before the parameter list.";
                // TODO(kai): other special cases for procedures without a name, make the compiler smarter!
                else
                {
                    // TODO(kai): Check if this token is a modifier that procedures accept. If it is, we can give better errors!

                    // If the token AFTER this is NOT an open bracket ( then we should just
                    //  stop trying to parse a procedure.
                    // Treat the `proc` keyword as a typo and just move on.
                    if (!CheckNext(TokenKind.OpenBracket))
                    {
                        // We don't have a name AND we can't yet see a parameter list.
                        // At this point we assume that a procedure is NOT what's coming up,
                        //  and ask the user if that was intended.
                        var failMessage = Message.UnnamedProcedure(kwProc.Span,
                            "Failed to parse a procedure declaration. Is `proc` a typo?");
                        Log.AddError(failMessage);
                        return null;
                    }
                    else // if (CheckNext(TokenKind.OpenBracket))
                    {
                        // Here we can see an open bracket, let's assume it's a parameter list.
                        // If that's the case then this was intended as the name.
                        // TODO(kai): We can check what kind of token this actually is and probably get even better messages out.
                        description = string.Format(CultureInfo.InvariantCulture,
                            "Invalid token for procedure name `{0}`.", Current.Image);
                        Advance();
                    }
                }
                // Log the not-so-failure messages here.
                var message = Message.UnnamedProcedure(kwProc.Span, description);
                Log.AddError(message);
            }

            ProcedureTypeInfo procType;
            // If we think there's a parameter list...
            if (Check(TokenKind.OpenBracket))
            {
                procType = ParseProcedureTypeInfo(out handleFailureMessage);
                // URGENT(kai): Is this the right thing to do here? I think so.
                if (procType == null)
                    return null;
            }
            else
            {
                var message = Message.MissingProcedureType(kwProc.Span, tkName.Image,
                    IsEndOfSource ? null : Current);
                Log.AddError(message);
                return null;
            }

            ProcedureBody procBody = null;
            if (!isExtern)
            {
                procBody = ParseProcedureBody(kwProc, tkName, out handleFailureMessage);
                if (procBody == null)
                    return null;
            }

            return new NodeProcedureDeclaration(kwProc, tkName, procType, procBody);
        }

        private ProcedureBody ParseProcedureBody(Token kwProc, Token tkName, out bool handleFailureMessage)
        {
            handleFailureMessage = true;

            // Check for `=`:
            if (Check(OperatorKind.Equal))
            {
                var tkEqual = Current;
                Advance(); // `=`
                var body = ParseExpression(out handleFailureMessage);
                if (body == null)
                    return null;
                // TODO(kai): I can shorten this to `if (!(body as NodeBlock)?.CanBeExpression ?? false)`
                if (body is NodeBlock && !(body as NodeBlock).CanBeExpression)
                {

                }
                return new ProcedureBody(tkEqual, body);
            }
            // If no `=`, check for `{`:
            else if (Check(TokenKind.OpenCurlyBracket))
            {
                // should always be a block here.
                var body = ParseExpression(out handleFailureMessage);
                if (body == null)
                    return null;
                return new ProcedureBody(null, body);
            }
            // Otherwise we messed up D:
            else
            {
                handleFailureMessage = false;
                var message = Message.MissingProcedureBody(kwProc.Span, tkName.Image,
                    IsEndOfSource ? null : Current);
                Log.AddError(message);
                return null;
            }

            // return null;
        }
        #endregion
    }
}
