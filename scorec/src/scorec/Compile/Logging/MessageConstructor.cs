using System.Diagnostics;
using System.Globalization;

namespace ScoreC.Compile.Logging
{
    using Source;

    sealed partial class Message
    {
        #region Lex Stage Errors
        public static Message TokenLexFailed(Span location)
        {
            var startChar = location.Map.GetSourceAtSpan(location);
#if DEBUG
            Debug.Assert(startChar != null);
#endif
            var description = string.Format(CultureInfo.InvariantCulture,
                "Failed to lex token at '{0}'.", startChar);
            return new Message(MessageCode.TokenLexFailed, location, description);
        }

        public static Message InvalidDigitInRadix(Span location, char digit, string image)
        {
#if DEBUG
            Debug.Assert(image != null);
#endif
            var description = string.Format(CultureInfo.InvariantCulture,
                "Invalid digit character '{0}' in integer literal {1}.", digit, image);
            return new Message(MessageCode.InvalidDigitInRadix, location, description);
        }

        // FIXME(kai): this needs to take an image, but I have to figure that out... :c
        public static Message InvalidDigitSeparatorLocation(Span location)
        {
            var description = string.Format(CultureInfo.InvariantCulture,
                "Invalid placement of digit separator character in integer literal.");
            return new Message(MessageCode.InvalidDigitSeparatorLocation, location, description);
        }

        public static Message UnexpectedEndOfSource(Span location, string description) =>
            new Message(MessageCode.UnexpectedEndOfSource, location, description);

        public static Message UnfinishedLiteral(Span location, string literalName)
        {
#if DEBUG
            Debug.Assert(literalName != null);
#endif
            var description = string.Format(CultureInfo.InvariantCulture,
                "Unfinished {0} literal.", literalName);
            return new Message(MessageCode.UnfinishedLiteral, location, description);
        }

        public static Message InvalidOperatorToken(Span location, string image)
        {
#if DEBUG
            Debug.Assert(image != null);
#endif
            var description = string.Format(CultureInfo.InvariantCulture,
                // TODO(kai): I don't like the period here, maybe do something about that.
                "Invalid operator token {0}.", image);
            return new Message(MessageCode.InvalidOperatorToken, location, description);
        }
        #endregion

        #region Parse Stage Errors
        public static Message NodeParseFailed(Token at)
        {
#if DEBUG
            Debug.Assert(at != null);
#endif
            var description = string.Format(CultureInfo.InvariantCulture,
                "Failed to parse at '{0}'.", at.Image);
            return new Message(MessageCode.NodeParseFailed, at.Span, description);
        }

        public static Message UnnamedProcedure(Span location, string description) =>
            new Message(MessageCode.UnnamedProcedure, location, description);

        public static Message TypeParseFailed(Token at)
        {
#if DEBUG
            Debug.Assert(at != null);
#endif
            var description = string.Format(CultureInfo.InvariantCulture,
                "Failed to parse a type at '{0}'.", at.Image);
            return new Message(MessageCode.TypeParseFailed, at.Span, description);
        }

        public static Message TypeParseFailed(Span location, string description) =>
            new Message(MessageCode.TypeParseFailed, location, description);

        public static Message MissingProcedureType(Span location, string name, Token other)
        {
#if DEBUG
            Debug.Assert(name != null);
            Debug.Assert(other != null);
#endif
            string description;
            if (other == null)
                description = string.Format(CultureInfo.InvariantCulture,
                    "Procedure `{0}` is missing a type, found end of source instead.", name);
            else description = string.Format(CultureInfo.InvariantCulture,
                   "Procedure `{0}` is missing a type, found `{1}` instead.", name, other.Image);
            return new Message(MessageCode.MissingProcedureType, location, description);
        }

        public static Message MissingProcedureBody(Span location, string name, Token other)
        {
#if DEBUG
            Debug.Assert(name != null);
            Debug.Assert(other != null);
#endif
            string description;
            if (other == null)
                description = string.Format(CultureInfo.InvariantCulture,
                    "Procedure `{0}` is missing a body, found end of source instead.", name);
            else description = string.Format(CultureInfo.InvariantCulture,
                   "Procedure `{0}` is missing a body, found `{1}` instead.", name, other.Image);
            return new Message(MessageCode.MissingProcedureBody, location, description);
        }

        public static Message MissingType(Span location, string description) =>
            new Message(MessageCode.MissingType, location, description);

        public static Message ExpressionParseFailed(Token at)
        {
#if DEBUG
            Debug.Assert(at != null);
#endif
            var description = string.Format(CultureInfo.InvariantCulture,
                "Failed to parse an expression at '{0}'.", at.Image);
            return new Message(MessageCode.ExpressionParseFailed, at.Span, description);
        }

        public static Message UnexpectedToken(Span location, string description) =>
            new Message(MessageCode.UnexpectedToken, location, description);

        public static Message UnexpectedToken(Span at, string foundImage, string expected)
        {
#if DEBUG
            Debug.Assert(foundImage != null);
            Debug.Assert(expected != null);
#endif
            var description = string.Format("{0} expected, found {1}.",
                expected, foundImage);
            return new Message(MessageCode.UnexpectedToken, at, description);
        }

        public static Message UnexpectedToken(Span at, string foundImage, string expected, string message)
        {
#if DEBUG
            Debug.Assert(foundImage != null);
            Debug.Assert(expected != null);
            Debug.Assert(message != null);
#endif
            var description = string.Format("{0} expected {1}, found {2}.",
                expected, message, foundImage);
            return new Message(MessageCode.UnexpectedToken, at, description);
        }

        public static Message InvalidCharacterLiteral(Token literalString)
        {
#if DEBUG
            Debug.Assert(literalString != null);
#endif
            var description = string.Format("The string literal {0} does not contain one unicode code point, so it is not a valid character literal.",
                literalString.Image);
            return new Message(MessageCode.InvalidCharacterLiteral, literalString.Span, description);
        }
        #endregion
    }
}
