using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    // FIXME(kai): Operator.GetInfix() might be great to be wrapped up in Token as well, looks ugly on its own here.

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
                    Log.AddError(start.Span, "Failed to parse at token `{0}`.", start.Image);
                    Advance(); // Skip this one plz.
                }
            }

            // get rid of data, we're done here.
            Map = null;
            Log = null;

            return ast;
        }

        private string EoSOrCurrentToken()
        {
            if (IsEndOfSource)
                return "end of source";
            return "`" + Current.Image + "`";
        }

        private Span EoSOrCurrentSpan()
        {
            if (IsEndOfSource)
                return Map.EndOfSourceSpan;
            return Current.Span;
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
            case TokenKind.Extern:
                Advance();
                if (!Check(TokenKind.Proc))
                {
                    handleFailureMessage = false;
                    Log.AddError(EoSOrCurrentSpan(),
                                 "Expected `proc` to start a procedure declaration following `extern`, found `{0}`.",
                                 EoSOrCurrentToken());
                    return null;
                }
                return ParseProcedureDeclaration(true, out handleFailureMessage);

            case TokenKind.Proc:
                return ParseProcedureDeclaration(false, out handleFailureMessage);

            case TokenKind.Let:
            case TokenKind.Var:
                return ParseBindingDeclaration(out handleFailureMessage);

            case TokenKind.Struct:
                return ParseStructDeclaration(out handleFailureMessage);

            default:
                var expr = ParseExpression(out handleFailureMessage);

                if (expr == null)
                    return null;

                if (Check(TokenKind.Assign))
                {
                    Advance(); // `=`

                    var value = ParseExpression(out handleFailureMessage);
                    if (value == null)
                    {
                        // TODO(kai): This format is used everywhere in the parser, should be factored into a method
                        if (handleFailureMessage)
                        {
                            handleFailureMessage = false;
                            Log.AddError(EoSOrCurrentSpan(),
                                         "Expected an expression following an assignment operator, found {0}.",
                                         EoSOrCurrentToken());
                        }
                        return null;
                    }

                    if (!expr.IsLValue)
                    {
                        handleFailureMessage = false;
                        Log.AddError(expr.Start, "Invalid assignment target, can only assign to l-values.");
                        return null;
                    }

                    return new NodeAssignment(expr, value);
                }

                return expr;
            }
        }

        #region Expressions
        private NodeExpression ParseExpression(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
            var lhs = ParsePrimaryExpression(out handleFailureMessage);
            // TODO(kai): check `as`, `when` etc.
            return ParseInfixOperations(out handleFailureMessage, lhs);
        }

        private NodeExpression ParseInfixOperations(out bool handleFailureMessage, NodeExpression lhs, int minPrecedence = 0)
        {
            handleFailureMessage = true;

            // This condition could probably be a local function (if I could factor that >= vs > into something I guess)
            while (!IsEndOfSource &&
                   Check(TokenKind.Operator) &&
                   Operator.IsInfix(Current.Image) &&
                   Operator.GetPrecedence(Current.Image) >= minPrecedence &&
                   Current.Span.Line == Previous.Span.EndLine)
            {
                var opToken = Current;
                Advance();
                // Get an expression without checking infix.
                var rhs = ParsePrimaryExpression(out handleFailureMessage);
                // This right here is a prime candidate for a local function. We do it twice, such copy-pasta..
                if (rhs == null)
                {
                    if (handleFailureMessage)
                    {
                        handleFailureMessage = false;
                        Log.AddError(opToken.Span,
                                     "Expected an expression for the right side of infix operator `{0}`.",
                                     opToken.Image);
                    }
                    return null;
                }
                // Make sure we don't have a semi colon.
                else
                {
                    var thisPrec = Operator.GetPrecedence(opToken.Image);
                    // Basically we do the same as above
                    // This time the precedence must be GREATER, but not equal.
                    while (!IsEndOfSource &&
                           Check(TokenKind.Operator) &&
                           Operator.IsInfix(Current.Image) &&
                           Operator.GetPrecedence(Current.Image) > thisPrec &&
                           Current.Span.Line == Previous.Span.EndLine)
                    {
                        // Our right side is now the result of the infix operation.
                        rhs = ParseInfixOperations(out handleFailureMessage, rhs, Operator.GetInfix(Current.Image).Precedence());
                        if (rhs == null)
                        {
                            if (handleFailureMessage)
                            {
                                handleFailureMessage = false;
                                Log.AddError(opToken.Span,
                                             "Expected an expression for the right side of infix operator `{0}`.",
                                             opToken.Image);
                            }
                            return null;
                        }
                    }
                }
                lhs = new NodeInfix(Operator.GetInfix(opToken.Image), lhs, rhs);
            }
            return lhs;
        }

        /// <summary>
        /// Attempts to parse the lowest level of expressions, including suffixes that attach to them.
        /// If any errors are encountered, this returns null.
        /// </summary>
        /// <param name="handleFailureMessage"></param>
        /// <returns></returns>
        private NodeExpression ParsePrimaryExpression(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
            if (IsEndOfSource)
                return null;

            NodeExpression result = null;

            if (Check(TokenKind.True) || Check(TokenKind.False))
            {
                var tkBoolLiteral = Current;
                Advance();
                result = new NodeBoolLiteral(tkBoolLiteral);
            }
            /* TODO(kai): This isn't a part of the base language, but it already exists. Add it back later.
            else if (Check(TokenKind.Auto))
            {
                var start = Current.Span;
                Advance();
                var target = ParsePrimaryExpression(out handleFailureMessage);
                if (target == null)
                    return null;
                return new NodeAutoCast(start, target);
            }
            */
            else if (CheckDirective("char"))
            {
                var start = Current.Span;
                Advance(); // `#char`

                if (!Check(TokenKind.String))
                {
                    handleFailureMessage = false;
                    Log.AddError(EoSOrCurrentSpan(),
                                 "Expected a string literal to follow the #char directive, found {0}.",
                                 EoSOrCurrentToken());
                    return null;
                }

                var tkLiteralString = Current;
                Advance(); // string literal

                var literalBytes = Encoding.Unicode.GetBytes(tkLiteralString.StringLiteral);
                var utf32 = Encoding.Convert(Encoding.Unicode, Encoding.UTF32, literalBytes);
                var charCount = Encoding.UTF32.GetCharCount(utf32);

                if (charCount > 1)
                {
                    handleFailureMessage = false;
                    Log.AddError(tkLiteralString.Span,
                                 "The given string literal does not contain a lone Unicode code point. Cannot convert to a character literal.");
                    return null;
                }

                var c = Encoding.UTF32.GetChars(utf32);

                uint literal;
                if (c.Length == 2)
                    literal = (uint)char.ConvertToUtf32(c[1], c[0]);
                else literal = (uint)c[0];
                
                result = new NodeCharLiteral(start, literal);
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
                    var start = Current.Span;
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
                        handleFailureMessage = false;
                        Log.AddError(EoSOrCurrentSpan(),
                                     "Expected `}` to close this block, found {0}.",
                                     EoSOrCurrentToken());
                        return null;
                    }

                    Advance(); // `}`

                    result = new NodeBlock(start, body);
                    break;
                default:
                    {
                        handleFailureMessage = false;
                        Log.AddError(Current.Span,
                                     "Failed to parse an expression at token `{0}`.",
                                     Current.Image);
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

        /// <summary>
        /// Attempts to parse a comma-separated list of expressions.
        /// This allows for an optional trailing comma and expects
        ///  to be stopped by a delimiter.
        /// 
        /// If any errors are encountered, an empty list is returned.
        /// 
        /// FIXME(kai): If I do multi-assignments then I'll need to remove
        ///  the requirement for a delimiter, I need to make it an option.
        /// </summary>
        /// <param name="allowTrailingComma"></param>
        /// <param name="closeDelimiter"></param>
        /// <param name="handleFailureMessage"></param>
        /// <returns></returns>
        private List<NodeExpression> ParseCommaSeparatedExpressions(bool allowTrailingComma, TokenKind closeDelimiter, out bool handleFailureMessage)
        {
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

                if (Check(closeDelimiter))
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
                        Log.AddError(Previous.Span,
                                     "Unexpected `,` found before closing delimiter `{0}`.",
                                     closeDelimiterImage);
                        return new List<NodeExpression>();
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
                        handleFailureMessage = false;
                        Log.AddError(EoSOrCurrentSpan(),
                                     "Expected `{0}`, found {1}.",
                                     closeDelimiterImage,
                                     EoSOrCurrentToken());
                        return new List<NodeExpression>();
                    }
                }
            }

            if (!Expect(closeDelimiter))
            {
                handleFailureMessage = false;
                Log.AddError(EoSOrCurrentSpan(),
                             "Expected `{0}`, found {1}.",
                             closeDelimiterImage,
                             EoSOrCurrentToken());
                return new List<NodeExpression>();
            }

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

            if (Check(TokenKind.As))
            {
                var tkAs = Current;
                Advance(); // `as`
                var typeInfo = ParseTypeInfo(out handleFailureMessage);
                if (typeInfo == null)
                {
                    result = null;
                    return;
                }
                result = new NodeExplicitCast(result, typeInfo);
                return;
            }

            switch (Current.Kind)
            {
            case TokenKind.OpenBracket:
                {
                    Advance();

                    var argumentList = ParseCommaSeparatedExpressions(false, TokenKind.CloseBracket, out handleFailureMessage);

                    if (argumentList == null)
                        result = null;
                    else result = new NodeInvocation(result, argumentList);
                    return;
                }
            case TokenKind.Dot:
                {
                    // TODO(kai): .type
                    Advance(); //  `.`
                    if (Check(TokenKind.Identifier))
                    {
                        result = new NodeFieldIndex(result, Current);
                        Advance();
                    }
                    else
                    {
                        handleFailureMessage = false;
                        Log.AddError(EoSOrCurrentSpan(),
                                     "Identifier expected for field index, found {0}",
                                     EoSOrCurrentToken());
                        result = null;
                    }
                    return;
                }
            }
        }
        #endregion

        /// <summary>
        /// Attempts to parse a type reference.
        /// If any errors are encountered, this returns null.
        /// </summary>
        /// <param name="handleFailureMessage"></param>
        /// <returns></returns>
        private TypeInfo ParseTypeInfo(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(!IsEndOfSource, "you're at the end of the source, buddy, stahp.");
#endif

            if (CheckOperator("*"))
            {
                var tkStar = Current;
                Advance();
                var type = ParseTypeInfo(out handleFailureMessage);
                if (type == null)
                    return null;
                return new PointerTypeInfo(tkStar, type);
            }
            else if (Check(TokenKind.BuiltinTypeName))
            {
                Advance();
                return BuiltinTypeInfo.Get(Previous.Image);
            }
            else
            {
                switch (Current.Kind)
                {
                case TokenKind.Identifier:
                    {
                        var path = new List<Token>();
                        while (!IsEndOfSource)
                        {
                            var name = Current;
                            if (!Expect(TokenKind.Identifier))
                            {
                                name = null;
                                handleFailureMessage = false;
                                Log.AddError(EoSOrCurrentSpan(),
                                             "Expected an identifier to continue the qualified type name, found {0}.",
                                             EoSOrCurrentToken());
                                return null;
                            }
                            path.Add(name);
                            if (!Check(TokenKind.Dot))
                                break;
                            Advance(); // `.`
                        }
                        return new QualifiedTypeInfo(path);
                    }
                #region case Tuple or Procedure Type:
                case TokenKind.OpenBracket:
                    {
                        var tkOpenBracket = Current;
                        Advance(); // `(`

                        var names = new List<Token>();
                        var types = new List<TypeInfo>();

                        // FIXME(kai): !!!!!!!! I think this needs to be a bit better plz help

                        while (!Check(TokenKind.CloseBracket))
                        {
                            Token tkName;
                            if (Check(TokenKind.Identifier) && CheckNext(TokenKind.Colon))
                            {
                                tkName = Current;
                                Advance(2);
                            }
                            // No name, we'll check for consistency later.
                            else tkName = null;
                            names.Add(tkName);

                            var type = ParseTypeInfo(out handleFailureMessage);
                            types.Add(type);

                            if (type == null)
                            {
                                if (Check(TokenKind.Comma))
                                {
                                    Log.AddError(Current.Span,
                                                 "Expected a parameter type in procedure type, found a comma.");
                                    // NOTE(kai): continue on to parse out the comma
                                }
                                else if (CheckNext(TokenKind.Comma))
                                {
                                    Log.AddError(Current.Span,
                                                 "Expected a parameter type in procedure type, found `{0}`.",
                                                 Current.Image);
                                    Advance();
                                    // NOTE(kai): continue on to parse out the comma
                                }
                                else
                                {
                                    handleFailureMessage = false;
                                    Log.AddError(Current.Span,
                                                 "Failed to parse parameter type in procedure type.");
                                    return null;
                                }
                            }

                            // If we hit a `)` then we can stop looping
                            if (Check(TokenKind.CloseBracket))
                                break;
                            // Otherwise we expect to keep going, need a comma.
                            else if (!Expect(TokenKind.Comma))
                            {
                                // TODO(kai): Check options here for better recovery/errors.
                                Log.AddError(EoSOrCurrentSpan(),
                                             "Expected `,` in procedure type, found {0}",
                                             EoSOrCurrentToken());
                                return null;
                            }
                        }

                        var tkCloseBracket = Current;
                        if (!Expect(TokenKind.CloseBracket))
                        {
                            Log.AddError(tkOpenBracket.Span,
                                "Missing `(` to match opening `)` in tuple or procedure type.");
                            return null;
                        }

                        // FIXME(kai): check that the type either always or never defines names.
                        var parameters = new List<Binding>();
                        var count = types.Count;
                        for (int i = 0; i < count; i++)
                            // TODO(kai): When we introduce default parameters, change 'null' to an actually parsed value.
                            parameters.Add(new Binding(names[i], types[i], null));

                        if (Check(TokenKind.GoesTo))
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
                                    Log.AddError(tkArrow.Span,
                                        "Procedure type is missing a return type.");
                                }
                                return null;
                            }

                            // TODO(kai): support multiple return values

                            var returns = new List<Binding>();
                            returns.Add(new Binding(null, returnType, null));

                            return new ProcedureTypeInfo(tkOpenBracket, tkCloseBracket, tkArrow, parameters, returns);
                        }
                        else
                        {
                            var returns = new List<Binding>();
                            returns.Add(new Binding(null, BuiltinTypeInfo.Get(BuiltinType.Void), null));

                            return new ProcedureTypeInfo(tkOpenBracket, tkCloseBracket, null, parameters, returns);
                        }
                    }
                    #endregion
                }
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

        private NodeBindingDeclaration ParseBindingDeclaration(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(Check(TokenKind.Let) || Check(TokenKind.Var), "Expected `let` or `var` to start binding declaration.");
#endif

            var start = Current.Span;
            var bindingKind = Current.Kind;

            Advance(); // `let` or `var`

            if (!Check(TokenKind.Identifier))
            {
                handleFailureMessage = false;
                Log.AddError(EoSOrCurrentSpan(),
                             "Expected identifier for binding name, found `{0}`.",
                             EoSOrCurrentToken());
                return null;
            }

            var tkIdentifier = Current;
            Advance();

            TypeInfo typeInfo = null;
            if (Check(TokenKind.Colon))
            {
                Advance();

                typeInfo = ParseTypeInfo(out handleFailureMessage);
                if (typeInfo == null)
                {
                    if (handleFailureMessage)
                    {
                        handleFailureMessage = false;
                        Log.AddError(EoSOrCurrentSpan(),
                                     "Expected type in binding declaration, found {0}.",
                                     EoSOrCurrentToken());
                    }
                    return null;
                }
            }

            NodeExpression value = null;
            if (Check(TokenKind.Assign))
            {
                Advance();

                value = ParseExpression(out handleFailureMessage);
                if (value == null)
                {
                    if (handleFailureMessage)
                    {
                        handleFailureMessage = false;
                        Log.AddError(EoSOrCurrentSpan(),
                                     "Expected expression value in binding declaration, found {0}.",
                                     EoSOrCurrentToken());
                    }
                    return null;
                }
            }

            return new NodeBindingDeclaration(start, bindingKind, new Binding(tkIdentifier, typeInfo, value));
        }

        /// <summary>
        /// Attempts to parse a procedure declaration + optional body,
        ///  depending on what modifiers exist.
        /// If any errors are encountered, this returns null.
        /// </summary>
        /// <param name="isExtern"></param>
        /// <param name="handleFailureMessage"></param>
        /// <returns></returns>
        private NodeProcedureDeclaration ParseProcedureDeclaration(bool isExtern, out bool handleFailureMessage)
        {
            // FIXME(kai): remove isExtern, replace with a Modifiers list or class, idunno.

            // NOTE(kai): this is only used where the parser think it can continue
            var errored = false;

            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(Check(TokenKind.Proc), "Expected `proc` to start procedure declaration.");
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
                    errored = true;

                    // TODO(kai): Check if this token is a modifier that procedures accept. If it is, we can give better errors!

                    // If the token AFTER this is NOT an open bracket ( then we should just
                    //  stop trying to parse a procedure.
                    // Treat the `proc` keyword as a typo and just move on.
                    if (!CheckNext(TokenKind.OpenBracket))
                    {
                        // We don't have a name AND we can't yet see a parameter list.
                        // At this point we assume that a procedure is NOT what's coming up,
                        //  and ask the user if that was intended.
                        Log.AddError(kwProc.Span, "Failed to parse a procedure declaration. Is `proc` a typo?");
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
                Log.AddError(kwProc.Span, description);
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
                handleFailureMessage = false;
                Log.AddError(kwProc.Span,
                             "Procedure is missing a type.");
                return null;
            }

            ProcedureBody procBody = null;
            if (!isExtern)
            {
                procBody = ParseProcedureBody(kwProc, tkName, out handleFailureMessage);
                if (procBody == null)
                    return null;
            }

            if (errored)
                return null;

            return new NodeProcedureDeclaration(kwProc, tkName, procType, procBody);
        }

        /// <summary>
        /// Attempts to parse a procedure body.
        /// If any errors are encountered, this returns null.
        /// </summary>
        /// <param name="kwProc"></param>
        /// <param name="tkName"></param>
        /// <param name="handleFailureMessage"></param>
        /// <returns></returns>
        private ProcedureBody ParseProcedureBody(Token kwProc, Token tkName, out bool handleFailureMessage)
        {
            handleFailureMessage = true;

            // Check for `=`:
            if (Check(TokenKind.Assign))
            {
                var tkEqual = Current;
                Advance(); // `=`
                var body = ParseExpression(out handleFailureMessage);
                if (body == null)
                    return null;
                // TODO(kai): I can shorten this to `if (!(body as NodeBlock)?.CanBeExpression ?? false)`
                if (body is NodeBlock && !(body as NodeBlock).CanBeExpression)
                {
                    // FIXME(kai): should this be a parse-time check for expression-ness? might be nice..
                }
                return new ProcedureBody(body);
            }
            // If no `=`, check for `{`:
            else if (Check(TokenKind.OpenCurlyBracket))
            {
                // should always be a block here.
                var body = ParseExpression(out handleFailureMessage);
                if (body == null)
                    return null;
                return new ProcedureBody(body);
            }
            // Otherwise we messed up D:
            else
            {
                handleFailureMessage = false;
                Log.AddError(kwProc.Span, "Procedure is missing a body.");
                return null;
            }

            // return null;
        }

        private NodeStructDeclaration ParseStructDeclaration(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(Check(TokenKind.Struct));
#endif
            var start = Current.Span;
            Advance(); // `struct`

            var name = Current;
            if (!Expect(TokenKind.Identifier))
            {
                name = null; // just in case
                handleFailureMessage = false;
                if (Check(TokenKind.OpenCurlyBracket))
                    Log.AddError(Previous.Span, "Missing an identifier for the name of this struct.");
                    // We continue because we think we found a struct declaration anyway, just without the name.
                else
                {
                    Log.AddError(Previous.Span,
                                 "Expected an identifier for the name of this struct, found {0}.",
                                 EoSOrCurrentToken());
                    return null;
                }
            }

            if (!Expect(TokenKind.OpenCurlyBracket))
            {
                handleFailureMessage = false;
                Log.AddError(EoSOrCurrentSpan(),
                             "Expected `{` to start struct field body, found {0}.",
                             EoSOrCurrentToken());
                if (!(Check(TokenKind.Identifier) && CheckNext(TokenKind.Colon)))
                    return null;
            }

            var fields = new List<Binding>();

            while (!IsEndOfSource && !Check(TokenKind.CloseCurlyBracket))
            {
                var tkName = Current;
                if (!Expect(TokenKind.Identifier))
                {
                    tkName = null;
                    handleFailureMessage = false;
                    Log.AddError(Current.Span, "Expected identifier for struct field name, found `{0}`.", Current.Image);
                    if (CheckNext(TokenKind.Colon))
                        Advance();
                    else continue;
                }

                if (!Expect(TokenKind.Colon))
                {
                    // Only error if we haven't already.
                    if (tkName != null)
                    {
                        handleFailureMessage = false;
                        Log.AddError(EoSOrCurrentSpan(),
                                     "Expected `:` to follow struct field name, found {0}.",
                                     EoSOrCurrentToken());
                        // continue on, maybe we can parse a type
                    }
                }

                var type = ParseTypeInfo(out handleFailureMessage);
                if (type == null && !handleFailureMessage)
                {
                    handleFailureMessage = false;
                    Log.AddError(EoSOrCurrentSpan(),
                                 "Failed to parse type for struct field at {0}.", EoSOrCurrentToken());
                }

                // TODO(kai): binding initializers plz
                fields.Add(new Binding(tkName, type, null));
            }

            if (!Expect(TokenKind.CloseCurlyBracket))
            {
                handleFailureMessage = false;
                Log.AddError(EoSOrCurrentSpan(),
                             "Expected `}` to end struct declaration, found {0}.",
                             EoSOrCurrentToken());
                return null;
            }

            return new NodeStructDeclaration(start, name, fields);
        }
        #endregion
    }
}
