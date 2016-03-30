using ScoreC.Compile.SyntaxTree;
using System.Diagnostics;

namespace ScoreC.Compile.Source
{
    // FIXME(kai): I fee like OperatorKind is a bad thing here, maybe even Keyword. Remove them, pleaseee <3!

    sealed class Token
    {
        #region Static Constructors
        public static Token New(Span span, TokenKind kind, string image)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(image), "Invalid null image!");
#endif
            var result = new Token(span, kind);
            result.Image = image;
            return result;
        }

        public static Token NewIdentifier(Span span, string identifier)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(identifier), "Invalid null identifier!");
            Debug.Assert(Source.Identifier.IsValid(identifier), "Given image isn't a valid identifier!");
#endif
            var result = new Token(span, TokenKind.Identifier);
            result.Identifier = identifier;
            result.Image = identifier;
            return result;
        }

        public static Token NewKeyword(Span span, string keyword)
        {
#if DEBUG
            Debug.Assert(Source.Identifier.IsKeyword(keyword), "Given image isn't a valid keyword!");
#endif
            var result = new Token(span, Source.Identifier.GetKeywordKind(keyword));
            result.Image = keyword;
            return result;
        }

        public static Token NewBuiltinTypeName(Span span, string typeName)
        {
#if DEBUG
            Debug.Assert(BuiltinTypeInfo.IsBulitinTypeName(typeName), "Given image isn't a valid builtin type name!");
#endif
            var result = new Token(span, TokenKind.BuiltinTypeName);
            result.Image = typeName;
            return result;
        }

        public static Token NewWildCard(Span span)
        {
            var result = new Token(span, TokenKind.WildCard);
            result.Image = "_";
            return result;
        }

        public static Token NewVarargs(Span span)
        {
            var result = new Token(span, TokenKind.Varargs);
            result.Image = "...";
            return result;
        }

        public static Token NewUninitialized(Span span)
        {
            var result = new Token(span, TokenKind.Uninitialized);
            result.Image = "---";
            return result;
        }

        public static Token NewOperator(Span span, string image)
        {
            var result = new Token(span, TokenKind.Operator);
            result.Image = image;
            return result;
        }

        public static Token NewDelimiter(Span span, char image)
        {
            TokenKind kind = TokenKind.None;
            switch (image)
            {
            case '(': kind = TokenKind.OpenBracket; break;
            case ')': kind = TokenKind.CloseBracket; break;
            case '[': kind = TokenKind.OpenSquareBracket; break;
            case ']': kind = TokenKind.CloseSquareBracket; break;
            case '{': kind = TokenKind.OpenCurlyBracket; break;
            case '}': kind = TokenKind.CloseCurlyBracket; break;
            case ',': kind = TokenKind.Comma; break;
            case ':': kind = TokenKind.Colon; break;
            case '.': kind = TokenKind.Dot; break;
            default:
#if DEBUG
                    Debug.Assert(false, "Given character is not a delimiter!!");
#endif
                    break;
            }
            var result = new Token(span, kind);
            result.Image = image.ToString();
            return result;
        }

        public static Token NewStringLiteral(Span span, string literal, string image)
        {
            var result = new Token(span, TokenKind.String);
            result.StringLiteral = literal;
            result.Image = image;
            return result;
        }

        public static Token NewIntegerLiteral(Span span, string literal, int radix, string image)
        {
            var result = new Token(span, TokenKind.Integer);
            result.NumericLiteral = literal;
            result.IntegerRadix = radix;
            result.Image = image;
            return result;
        }

        public static Token NewFloatLiteral(Span span, string literal, string image)
        {
            var result = new Token(span, TokenKind.Real);
            result.NumericLiteral = literal;
            result.Image = image;
            return result;
        }

        public static Token NewDirective(Span span, string directive)
        {
            var result = new Token(span, TokenKind.Directive);
            result.Directive = directive;
            result.Image = "#" + directive;
            return result;
        }
        #endregion

        /// <summary>
        /// The span this token covers.
        /// </summary>
        public Span Span { get; private set; }
        /// <summary>
        /// The human-readable image of this Token.
        /// </summary>
        public string Image { get; private set; } = null;

        #region Kinds
        /// <summary>
        /// The kind of Token this is.
        /// </summary>
        public TokenKind Kind { get; private set; }
        #endregion

        #region Literal Values
        /// <summary>
        /// The name of the identifier this Token represents.
        /// This will be null if this Token does not represent an identifier.
        /// </summary>
        public string Identifier { get; private set; } = null;
        /// <summary>
        /// The bool value this Token represents.
        /// This will be false if this Token does not represent a bool literal.
        /// </summary>
        public bool Bool => Kind == TokenKind.True;
        /// <summary>
        /// The numeric literal this Token represents.
        /// This is used for both integers and floats.
        /// We don't parse numbers because we support numeric values outside
        ///  the range of valid numbers in C#.
        /// C# has a max of 64 bits of storage, we're supporting up to 128.
        /// </summary>
        public string NumericLiteral { get; private set; } = "";
        /// <summary>
        /// The radix of the integer this Token represents.
        /// This will be 10 if this Token does not represent an integer literal.
        /// </summary>
        public int IntegerRadix { get; private set; } = 10;
        /// <summary>
        /// The string value this Token represents.
        /// This will be an empty string if this Token does not represent a string literal.
        /// </summary>
        public string StringLiteral { get; private set; } = "";
        /// <summary>
        /// The name of the directive this Token represents.
        /// This will be an empty string if this Token does not represent a directive.
        /// </summary>
        public string Directive { get; private set; } = "";
        #endregion

        #region Token Types Checks
        /// <summary>
        /// Determines if this Token is an identifier Token.
        /// If this is true, the Identifier property will not return null.
        /// </summary>
        public bool IsIdentifier => Kind == TokenKind.Identifier;
        /// <summary>
        /// Determines if this Token is a keyword Token.
        /// If this is true, the Keyword property will not return KeywordKind.None.
        /// </summary>
        public bool IsKeyword => Kind == TokenKind.Keyword;
        /// <summary>
        /// Determines if this Token is an operator Token.
        /// </summary>
        public bool IsOperator => Kind == TokenKind.Operator;
        #endregion

        private Token(Span span, TokenKind kind)
        {
            Span = span;
            Kind = kind;
        }

        public override string ToString() => Image;
    }
}
