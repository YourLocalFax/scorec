using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ScoreC.Compile.Source
{
    using Logging;

    sealed partial class Lexer
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public List<Token> GetTokens(Log log, SourceMap map)
        {
#if DEBUG
            Debug.Assert(log != null, "Log must not be null!");
            Debug.Assert(map != null, "Map must not be null!");
#endif
            // Initialize the data, we're ready to go! (probably...)
            Log = log;
            Map = map;

            sourceCached = Map.Source;
            sourceOffset = 0;

            // Begin!
            var tokens = new List<Token>();

            bool handleFailureMessage;
            EatWhiteSpace();
            while (!IsEndOfSource)
            {
                var start = GetSpan();
                var token = GetToken(start, out handleFailureMessage);
                // NOTE(kai): I actually need to define what this stuff means, I think failed is what should print the message yeah...
                // If failed is false, then the message is handled elsewhere.
                if (token != null)
                    tokens.Add(token);
                else if (handleFailureMessage)
                {
                    var message = Message.TokenLexFailed(start);
                    Log.AddError(message);
                    Advance();
                }
                EatWhiteSpace();
            }

            // Reset this lexer, we're done for now.
            sourceCached = null;
            sourceOffset = 0;

            Map = null;
            Log = null;

            // Exit!

            return tokens;
        }

        private Token GetToken(Span start, out bool failed)
        {
            failed = false;
#if DEBUG
            Debug.Assert(!IsEndOfSource, "No Tokens!! Called at end of source!");
#endif
            // NOTE(kai): Could use matchest, but this seems to be a little less verbose, though more confusing I guess.
            if (Current == '/' && HasNext && (Next == '#' || Next == '*'))
            {
                if (Next == '#')
                    EatLineComment();
                else EatBlockComment();
                EatWhiteSpace();
                if (IsEndOfSource)
                    return null;
                return GetToken(GetSpan(), out failed);
            }

            if (Matches("..."))
            {
                start = GetSpan();
                Advance(3);
                return Token.NewVarargs(GetSpan(start));
            }
            else if (Matches("---"))
            {
                start = GetSpan();
                Advance(3);
                return Token.NewUninitialized(GetSpan(start));
            }

            if (Current == '.' && HasNext && Next != '.')
            {
                var span = GetSpan();
                Advance();
                return Token.NewDelimiter(span, '.');
            }

            if (Identifier.IsStart(Current))
                return GetIdentifierOrKeyword();
            else if (Operator.IsOperatorStart(Current))
                return GetOperator();
            else if (char.IsDigit(Current))
                return GetNumericLiteral();

            // TODO(kai): Lex compiler directives like #if and #run

            switch (Current)
            {
                case '(': case ')':
                case '[': case ']':
                case '{': case '}':
                case ',':
                    Advance();
                    return Token.NewDelimiter(start, Previous);
                case '`':  return GetStringLiteral(start, '`', true);
                case '"':  return GetStringLiteral(start, '"');
                case '\'': return GetCharacterLiteral(start);
                default: break;
            }

            failed = true;
            Advance(); // Skip this token since we failed on it.
            return null;
        }

        private Token GetIdentifierOrKeyword()
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource, "Not enough characters exist to start an identifier!");
            Debug.Assert(Identifier.IsStart(Current), "Character (" + Current + ") doesn't start an identifier!");
#endif
            var start = GetSpan();
            // Make sure the string buffer is cleared.
            ClearBuffer();
            // We know the current character is an identifier start, so bump it.
            Bump();
            // For as long as characters exist that are valid identifier parts, bump them.
            while (!IsEndOfSource && Identifier.IsPart(Current))
                Bump();
            // This is the image we've created! Art!
            var image = GetStringFromBuffer();
            // Create the token woooo
            if (image == "_")
                return Token.NewWildCard(start);
            else if (Identifier.IsKeyword(image))
                return Token.NewKeyword(GetSpan(start), image);
            else return Token.NewIdentifier(GetSpan(start), image);
        }

        private Token GetStringLiteral(Span start, char delimiter, bool isVerbatim = false)
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource, "No characters left to be a string literal!");
            Debug.Assert(Current == delimiter, "Missing open quote for string literal!");
#endif
            Advance(); // open delimiter

            ClearBuffer();
            while (!IsEndOfSource)
            {
                if (Current == delimiter)
                    break;
                else if (Current == '\\')
                {
                    if (isVerbatim)
                    {
                        if (HasNext && Next == '`')
                            Advance();
                        Bump();
                    }
                    else
                    {
                        var escapedChar = GetEscapedCharacter();
                        if (!escapedChar.HasValue)
                            // NOTE(kai): Append a dummy value if the escape fails.
                            Append('\0');
                        else Append(escapedChar.Value);
                    }
                }
                else if (Current == '\n')
                {
                    if (isVerbatim)
                        Bump();
                    else
                    {
                        var message = Message.UnfinishedLiteral(start, "string");
                        Log.AddError(message);
                        return null;
                    }
                }
                else Bump();
            }

            if (IsEndOfSource)
            {
                var message = Message.UnexpectedEndOfSource(GetSpan(start), "Unfinished string literal, found end of source.");
                Log.AddError(message);
                return null;
            }

            if (!Expect(delimiter)) // close delimiter
            {
                var message = Message.UnfinishedLiteral(start, "string");
                Log.AddError(message);
                return null;
            }

            var span = GetSpan(start);
            var literal = GetStringFromBuffer();
            var image = Map.GetSourceAtSpan(span);

            return Token.NewStringLiteral(span, literal, image);
        }

        private Token GetCharacterLiteral(Span start)
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource, "No characters left to be a character literal!");
            Debug.Assert(Current == '\'', "Missing open quote for character literal!");
#endif
            Advance(); // open '

            if (IsEndOfSource)
            {
                var message = Message.UnexpectedEndOfSource(GetSpan(start), "Unfinished character literal, found end of source.");
                Log.AddError(message);
                return null;
            }

            uint value;

            if (Current == '\\')
            {
                var escaped = GetEscapedCharacter();
                if (!escaped.HasValue)
                    // NOTE(kai): GetEscapedCharacters will log errors if it fails, so don't do that here.
                    value = 0;
                else value = escaped.Value;
            }
            else
            {
                value = Current;
                Advance();
            }

            if (IsEndOfSource)
            {
                var message = Message.UnexpectedEndOfSource(GetSpan(start), "Unfinished character literal, found end of source.");
                Log.AddError(message);
                return null;
            }


            if (!Expect('\'')) // close '
            {
                var message = Message.UnfinishedLiteral(start, "character");
                Log.AddError(message);
                return null;
            }

            var span = GetSpan(start);
            var image = Map.GetSourceAtSpan(span);

            return Token.NewCharacterLiteral(span, value, image);
        }

        private char? GetEscapedCharacter()
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource, "No characters left to be a character!");
            Debug.Assert(Current == '\\', "Missing backslash for escaped character!");
#endif
            Advance();

            if (IsEndOfSource)
                return null;

            switch (Current)
            {
                case 'a':  Advance(); return '\a';
                case 'b':  Advance(); return '\b';
                case 'f':  Advance(); return '\f';
                case 'n':  Advance(); return '\n';
                case 'r':  Advance(); return '\r';
                case 't':  Advance(); return '\t';
                case 'v':  Advance(); return '\v';
                case '0':  Advance(); return '\0';      
                case '\'': Advance(); return '\'';
                case '"':  Advance(); return '"' ;
                case '\\': Advance(); return '\\';
                // URGENT(kai): Include octal/hexadecimal escapes.
                // These escapes can log errors.
                default:
                    Advance();
                    return Current;
            }
        }

        // URGENT(kai): refactor this plz, pretty ugly
        private Token GetOperator()
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource, "Not enough characters exist to start an operator!");
            Debug.Assert(Operator.IsOperatorStart(Current), "Current character '" + Current + "' is not an operator start.");
#endif
            var start = GetSpan();
            var operators = Operator.Operators;

            ClearBuffer();

            int charCount = 0;
            while (!IsEndOfSource)
            {
                string opImage;

                // Comment is starting here, so just stop plz
                if (Matches("/#") || Matches("/*"))
                {
                    opImage = GetStringFromBuffer();
                    goto ret_token;
                }
                var nextOps = operators.Where(op => op.Length > charCount && op[charCount] == Current).ToArray();
                // We have a match!
                if (nextOps.Length == 1 && nextOps[0].Length == charCount + 1)
                {
                    Advance();
                    opImage = nextOps[0];
                    goto ret_token;
                }
                // This character ends whatever operator we had going, so return that.
                else if (nextOps.Length == 0)
                {
                    opImage = GetStringFromBuffer();
                    foreach (var op in operators)
                        if (op == opImage)
                            goto ret_token;
                    // bad token, onu
                    var span = GetSpan(start);
                    var message = Message.InvalidOperatorToken(span, Map.GetSourceAtSpan(span));
                    Log.AddError(message);
                    return null;
                }

                operators = nextOps;

                Bump();
                charCount++;
                goto cont;

                ret_token:

                var kind = Operator.GetKindFromOperator(opImage);
                return Token.NewOperator(GetSpan(start), opImage, kind);

                cont: ;
            }

            // bad token, onu
            {
                var span = GetSpan(start);
                var message = Message.InvalidOperatorToken(span, GetStringFromBuffer());
                Log.AddError(message);
                return null;
            }
        }

        /// <summary>
        /// Will get a numeric literal, be it int or float, in all formats.. yay.
        /// TODO(kai): Maybe split this up if possible. This is gonna be big.
        /// </summary>
        /// <returns></returns>
        private Token GetNumericLiteral()
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource, "No characters left for a numeric literal!");
            Debug.Assert(char.IsDigit(Current), "Numeric literals must start with a digit, not '" + Current + "'!");
#endif
            var start = GetSpan();

            // If we see that this is CLEARLY a float, we change this
            TokenKind kind = TokenKind.Integer;
            int iRadix = 10;

            ClearBuffer();

            // FIRST, check for a base prefix (0x, 0b)
            if (Matches("0x") || Matches("0b"))
            {
                iRadix = (Next == 'x' || Next == 'X') ? 16 : 2;
                Advance(2);

                while (!IsEndOfSource)
                {
                    // FIXME(kai): make sure the placement of _'s is valid!
                    if (!(char.IsDigit(Current) || Identifier.IsPart(Current) || Current == '_'))
                        break;

                    if (Current == '_')
                    {
                        if (buffer.Length == 0 || !(HasNext && (char.IsDigit(Next) || Identifier.IsPart(Next))))
                        {
                            // FIXME(kai): There are multiple invalid locations, we should be specific!
                            var message = Message.InvalidDigitSeparatorLocation(start);
                            Log.AddError(message);
                        }
                        Advance();
                    }
                    else Bump();
                }
            }
            else
            {
                while (!IsEndOfSource)
                {
                    // If this is a dot, it only matters if we're an integer.
                    // If this is a float, we stop here.
                    if (Current == '.' && kind == TokenKind.Integer)
                    {
                        // A float literal must have a digit after the decimal place.
                        // If no digit is present, break. This will just have to be an integer literal.
                        if (!HasNext || !char.IsDigit(Next))
                            break;
                        Bump(); // add the dot to the literal
                        kind = TokenKind.Real;
                    }
                    else if ((Current == 'e' || Current == 'E') && HasNext && (Next == '-' || char.IsDigit(Next)))
                    {
                        Bump(); // add the 'e' or 'E'
                        if (Current == '-')
                            Bump(); // Only worry about the - because we ignore it otherwise.
                        kind = TokenKind.Real;
                    }
                    else
                    {
                        // NOTE(kai): This DOESN'T check that we have JUST digits, because something like 10asdf should
                        //  be considered as a single token, even though it's clearly wrong.
                        // If we didn't do this, then 10asdf would appear as two tokens, 10 and asdf.
                        if (!(char.IsDigit(Current) || Identifier.IsPart(Current)))
                            break;
                        Bump(); // add whatever this is to the literal
                    }
                }
            }

            var span = GetSpan(start);
            var literal = GetStringFromBuffer();

            // NOTE(kai): We DON'T parse these, because LLVM needs to do that.
            // If we parse them here, then we're limited to 64 bits because C# and yay >.>

            if (kind == TokenKind.Integer)
            {
                var image = Map.GetSourceAtSpan(span);
                foreach (var c in literal)
                {
                    if (!c.IsDigitInRadix(iRadix))
                    {
                        var message = Message.InvalidDigitInRadix(span, c, image);
                        Log.AddError(message);
                    }
                }
                return Token.NewIntegerLiteral(span, literal, iRadix, image);
            }
            else
            {
#if DEBUG
                Debug.Assert(kind == TokenKind.Real, "TokenKind should ONLY be float here you dumbass");
#endif
                // FIXME(kai): verify the literal!! Don't make LLVM do it later! This is a lexical check, it should be done in the lexer.
                // Do float validity checks here
                return Token.NewFloatLiteral(span, literal, Map.GetSourceAtSpan(span));
            }
        }
    }
}
