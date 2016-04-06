using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ScoreC.Compile.Source
{
    using Logging;
    using SyntaxTree;
    using System;

    sealed partial class Lexer
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public List<Token> GetTokens(SourceMap map)
        {
#if DEBUG
            Debug.Assert(map != null, "Map must not be null!");
#endif
            // Initialize the data, we're ready to go! (probably...)
            this.map = map;

            sourceCached = this.map.Source;
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
                    this.log.AddError(start, "Failed to lext token at `{0}`.", Current);
                    if (IsEndOfSource)
                        break;
                    Advance();
                }
                EatWhiteSpace();
            }

            this.map.EndOfSourceSpan = GetSpan();

            // Reset this lexer, we're done for now.
            sourceCached = null;
            sourceOffset = 0;

            this.map = null;

            // Exit!

            return tokens;
        }

        private Token GetToken(Span start, out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(!IsEndOfSource, "No Tokens!! Called at end of source!");
#endif
            // NOTE(kai): Could use matchest, but this seems to be a little less verbose, though more confusing I guess.
            if (Matches("/#") || Matches("/*"))
            {
                handleFailureMessage = false;
                if (Next == '#')
                    EatLineComment();
                else EatBlockComment();
                EatWhiteSpace();
                if (IsEndOfSource)
                    return null;
                return GetToken(GetSpan(), out handleFailureMessage);
            }

            if (Identifier.IsStart(Current))
                return GetIdentifierOrKeyword();
            //else if (Operator.IsOperatorStart(Current))
            //    return GetOperator();
            else if (char.IsDigit(Current))
                return GetNumericLiteral(out handleFailureMessage);

            // TODO(kai): Lex compiler directives like #if and #run

            switch (Current)
            {
            case '(': case ')':
            case '[': case ']':
            case '{': case '}':
            case ',': case ':':
                    Advance();
                    return Token.NewDelimiter(start, Previous);
            // TODO(kai): Not sure if we want two different delimiters for strings, might be able to use ` for something else..
            case '`': return GetStringLiteral(start, '`', true, out handleFailureMessage);
            case '"': return GetStringLiteral(start, '"', false, out handleFailureMessage);
            case '#': return GetDirective(start, out handleFailureMessage);
            case '.':
                Advance();
                if (Check('.'))
                {
                    Advance();
                    if (Check('.'))
                    {
                        Advance();
                        return Token.NewVarargs(GetSpan(start));
                    }
                    return Token.New(GetSpan(start), TokenKind.RangeOf, "..");
                }
                return Token.NewDelimiter(start, '.');
            case '=':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "==");
                }
                return Token.New(start, TokenKind.Assign, "=");
            case '>':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), ">=");
                }
                else if (Check('>'))
                {
                    Advance();
                    if (Check('='))
                    {
                        Advance();
                        return Token.NewOperator(GetSpan(start), ">>=");
                    }
                    return Token.NewOperator(GetSpan(start), ">>");
                }
                return Token.NewOperator(start, ">");
            case '<':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "<=");
                }
                else if (Check('<'))
                {
                    Advance();
                    if (Check('='))
                    {
                        Advance();
                        return Token.NewOperator(GetSpan(start), "<<=");
                    }
                    return Token.NewOperator(GetSpan(start), "<<");
                }
                else if (Check('>'))
                {
                    Advance();
                    if (Check('='))
                    {
                        Advance();
                        return Token.NewOperator(GetSpan(start), "<>=");
                    }
                    return Token.NewOperator(GetSpan(start), "<>");
                }
                return Token.NewOperator(start, "<");
            case '!':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "!=");
                }
                else if (Check('>'))
                {
                    Advance();
                    if (Check('='))
                    {
                        Advance();
                        return Token.NewOperator(GetSpan(start), "!>=");
                    }
                    return Token.NewOperator(start, "!>");
                }
                else if (Check('<'))
                {
                    Advance();
                    if (Check('='))
                    {
                        Advance();
                        return Token.NewOperator(GetSpan(start), "!<=");
                    }
                    else if (Check('>'))
                    {
                        Advance();
                        if (Check('='))
                        {
                            Advance();
                            return Token.NewOperator(GetSpan(start), "!<>=");
                        }
                        return Token.NewOperator(GetSpan(start), "!<>");
                    }
                    return Token.NewOperator(GetSpan(start), "!<");
                }
                return Token.NewOperator(start, "!");
            case '+':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "+=");
                }
                return Token.NewOperator(start, "+");
            case '-':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "-=");
                }
                else if (Check('>'))
                {
                    Advance();
                    return Token.New(GetSpan(start), TokenKind.GoesTo, "->");
                }
                return Token.NewOperator(start, "-");
            case '*':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "*=");
                }
                return Token.NewOperator(start, "*");
            case '/':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "/=");
                }
                return Token.NewOperator(start, "/");
            case '%':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "%=");
                }
                return Token.NewOperator(start, "%");
            case '\\':
                Advance();
                if (Check('='))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), @"\=");
                }
                return Token.NewOperator(start, @"\");
            case '&':
                Advance();
                if (Check('&'))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "&&");
                }
                return Token.NewOperator(start, "&");
            case '|':
                Advance();
                if (Check('|'))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "||");
                }
                return Token.NewOperator(start, "|");
            case '~':
                Advance();
                if (Check('~'))
                {
                    Advance();
                    return Token.NewOperator(GetSpan(start), "~~");
                }
                return Token.NewOperator(start, "~");
            case '^':
                Advance();
                return Token.NewOperator(GetSpan(start), "^");
            default: break;
            }

            handleFailureMessage = true;
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
            // Console.WriteLine(string.Format("`{0}` is {1}a keyword.", image, Identifier.IsKeyword(image) ? "" : "not "));
            // Create the token woooo
            if (image == "_")
                return Token.NewWildCard(start);
            else if (Identifier.IsKeyword(image))
                return Token.NewKeyword(GetSpan(start), image);
            else if (BuiltinTypeInfo.IsBulitinTypeName(image))
                return Token.NewBuiltinTypeName(GetSpan(start), image);
            else return Token.NewIdentifier(GetSpan(start), image);
        }

        private Token GetStringLiteral(Span start, char delimiter, bool isVerbatim, out bool handleFailureMessage)
        {
            handleFailureMessage = true;
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
                        handleFailureMessage = false;
                        log.AddError(start, "Unfinished string literal.");
                        return null;
                    }
                }
                else Bump();
            }

            if (IsEndOfSource)
            {
                handleFailureMessage = false;
                log.AddError(GetSpan(start), "Unfinished string literal, found end of source.");
                return null;
            }

            if (!Expect(delimiter)) // close delimiter
            {
                handleFailureMessage = false;
                log.AddError(start, "Unfinished string literal.");
                return null;
            }

            var span = GetSpan(start);
            var literal = GetStringFromBuffer();
            var image = map.GetSourceAtSpan(span);

            return Token.NewStringLiteral(span, literal, image);
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

        private Token GetDirective(Span start, out bool handleFailureMessage)
        {
            handleFailureMessage = true;
#if DEBUG
            Debug.Assert(!IsEndOfSource, "Not enough characters exist to start a directive!");
            Debug.Assert(Current == '#', "Current character '" + Current + "' is not a `#`.");
#endif

            Advance(); // `#`
            if (IsEndOfSource)
            {
                handleFailureMessage = false;
                log.AddError(start, "Expected identifier for directive name, found end of source.");
                return null;
            }

            var name = GetIdentifierOrKeyword();
            // Keywords can technically be directives, though they probably wont.
            var directive = name.Image;

            return Token.NewDirective(GetSpan(start), directive);
        }

        /// <summary>
        /// Will get a numeric literal, be it int or float, in all formats.. yay.
        /// TODO(kai): Maybe split this up if possible. This is gonna be big.
        /// </summary>
        /// <returns></returns>
        private Token GetNumericLiteral(out bool handleFailureMessage)
        {
            handleFailureMessage = true;
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
                            handleFailureMessage = false;
                            // FIXME(kai): There are multiple invalid locations, we should be specific!
                            log.AddError(start, "Invalid digit separator location. FIXME(kai): FIX THIS ERROR IT LOOKS BAD.");
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
                var image = map.GetSourceAtSpan(span);
                foreach (var c in literal)
                {
                    if (!c.IsDigitInRadix(iRadix))
                    {
                        handleFailureMessage = false;
                        log.AddError(span, "Invalid digit `{0}` in radix {1}.", c, iRadix);
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
                return Token.NewFloatLiteral(span, literal, map.GetSourceAtSpan(span));
            }
        }
    }
}
