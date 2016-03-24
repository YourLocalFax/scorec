using System;

namespace ScoreC.Compile.SyntaxTree
{
    using Logging;
    using Source;

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

        private void Advance(int count)
        {
            for (var i = 0; i < count; i++)
                Advance();
        }

        #region bool Check(kind)
        private bool Check(TokenKind tokenKind) =>
            !IsEndOfSource && Current.Kind == tokenKind;

        private bool Check(Keyword keyword) =>
            !IsEndOfSource && Current.Keyword == keyword;

        private bool Check(OperatorKind operatorKind) =>
            !IsEndOfSource && Current.OperatorKind == operatorKind;
        #endregion

        #region bool CheckNext(kind)
        private bool CheckNext(TokenKind tokenKind) =>
            HasNext && Next.Kind == tokenKind;

        private bool CheckNext(Keyword keyword) =>
            HasNext && Next.Keyword == keyword;

        private bool CheckNext(OperatorKind operatorKind) =>
            HasNext && Next.OperatorKind == operatorKind;
        #endregion

        #region bool Expect(kind)
        private bool Expect(TokenKind tokenKind)
        {
            if (!Check(tokenKind))
                return false;
            Advance();
            return true;
        }

        private bool Expect(Keyword keyword)
        {
            if (!Check(keyword))
                return false;
            Advance();
            return true;
        }

        private bool Expect(OperatorKind operatorKind)
        {
            if (!Check(operatorKind))
                return false;
            Advance();
            return true;
        }
        #endregion

        #region bool ExpectOrError(kind, message)
        private bool ExpectOrError(TokenKind tokenKind, Message message)
        {
            if (Expect(tokenKind))
                return true;
            Log.AddError(message);
            return false;
        }

        private bool ExpectOrError(Keyword keyword, Message message)
        {
            if (Expect(keyword))
                return true;
            Log.AddError(message);
            return false;
        }

        private bool ExpectOrError(OperatorKind operatorKind, Message message)
        {
            if (Expect(operatorKind))
                return true;
            Log.AddError(message);
            return false;
        }
        #endregion
    }
}
