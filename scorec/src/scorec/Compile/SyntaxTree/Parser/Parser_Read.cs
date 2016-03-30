using System;

namespace ScoreC.Compile.SyntaxTree
{
    using Logging;
    using Source;
    using System.Diagnostics;

    sealed partial class Parser
    {
        /// <summary>
        /// Determines if the end of the source file has been reached.
        /// </summary>
        private bool IsEndOfSource => tokenOffset >= tokensCached.Count;
        /// <summary>
        /// Returns the current token if it exists.
        /// </summary>
        private Token Current
        {
            get
            {
                if (tokensCached == null)
                    throw new InvalidOperationException("This parser has not been started, cannot access current token!");
                if (IsEndOfSource)
                    throw new InvalidOperationException("There is no current token, please use IsEndOfSource to check for this!");
                return tokensCached[tokenOffset];
            }
        }
        /// <summary>
        /// Returns true if there exists a token before the current one, false otherwise.
        /// </summary>
        private bool HasPrevious => tokensCached != null && tokenOffset > 0;
        /// <summary>
        /// Returns the token before the current one if it exists.
        /// </summary>
        private Token Previous
        {
            get
            {
                if (tokensCached == null)
                    throw new InvalidOperationException("This parser has not been started, cannot access previous token!");
                if (!HasPrevious)
                    throw new InvalidOperationException("There is no previous token, please use HasPrevious to check for this!");
                return tokensCached[tokenOffset - 1];
            }
        }
        /// <summary>
        /// Returns true if there exists a token after the current one, false otherwise.
        /// </summary>
        private bool HasNext => tokensCached != null && tokenOffset <= tokensCached.Count - 1;
        /// <summary>
        /// Returns the token after the current one if it exists.
        /// </summary>
        private Token Next
        {
            get
            {
                if (tokensCached == null)
                    throw new InvalidOperationException("This parser has not been started, cannot access next token!");
                if (!HasNext)
                    throw new InvalidOperationException("There is no next token, please use HasNext to check for this!");
                return tokensCached[tokenOffset + 1];
            }
        }

        /// <summary>
        /// Consumes the current token, moving to the next.
        /// </summary>
        private void Advance() =>
            tokenOffset++;

        /*
        private void Advance(string image)
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource);
            Debug.Assert(CheckOperator(image));
#endif
            if (Current.Image == image)
                Advance();
            else
            {
                // If we don't need this we can leave it commented just in case, but should still remove this method.
                throw new Exception("DO WE NEED THIS OR NAH. IF NOT REMOVE THIS METHOD AND CHANGE .StartsWith AND CHANGE IT TO ==");

                var oldToken = Current;

                var newCurrent = Token.NewOperator(oldToken.Span, image);

                var newSpan = new Span(oldToken.Span.Map, oldToken.Span.Line, oldToken.Span.Column + image.Length, oldToken.Span.EndLine, oldToken.Span.EndColumn);
                var newImage = oldToken.Image.Substring(image.Length);
                var newNext = Token.NewOperator(newSpan, newImage);

                Map.Tokens[tokenOffset] = newCurrent;
                Map.Tokens.Insert(tokenOffset + 1, newNext);
            }
        }
        */

        private void Advance(int count)
        {
            for (var i = 0; i < count; i++)
                Advance();
        }

        #region bool Check(kind)
        private bool Check(TokenKind tokenKind) =>
            !IsEndOfSource && Current.Kind == tokenKind;

        private bool CheckOperator(string image)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(image));
#endif
            return !IsEndOfSource && Current.IsOperator && Current.Image == image;
        }

        private bool CheckDirective(string directive)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(directive));
#endif
            return !IsEndOfSource && Current.Kind == TokenKind.Directive && Current.Directive == directive;
        }
        #endregion

        #region bool CheckNext(kind)
        private bool CheckNext(TokenKind tokenKind) =>
            HasNext && Next.Kind == tokenKind;
        #endregion

        #region bool Expect(kind)
        private bool Expect(TokenKind tokenKind)
        {
            if (!Check(tokenKind))
                return false;
            Advance();
            return true;
        }
        #endregion
    }
}
