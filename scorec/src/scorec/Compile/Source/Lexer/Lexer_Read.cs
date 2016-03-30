using System;

namespace ScoreC.Compile.Source
{
    sealed partial class Lexer
    {
        /// <summary>
        /// Determines if the end of the source file has been reached.
        /// </summary>
        private bool IsEndOfSource => sourceOffset >= sourceCached.Length;
        /// <summary>
        /// Returns the current character if it exists.
        /// </summary>
        private char Current
        {
            get
            {
                if (sourceCached == null)
                    throw new InvalidOperationException("This lexer has not been started, cannot access current character!");
                if (IsEndOfSource)
                    throw new InvalidOperationException("There is no current character, please use IsEndOfSource to check for this!");
                return sourceCached[sourceOffset];
            }
        }
        /// <summary>
        /// Returns true if there exists a character before the current one, false otherwise.
        /// </summary>
        private bool HasPrevious => sourceCached != null && sourceOffset > 0;
        /// <summary>
        /// Returns the character before the current one if it exists.
        /// </summary>
        private char Previous
        {
            get
            {
                if (sourceCached == null)
                    throw new InvalidOperationException("This lexer has not been started, cannot access previous character!");
                if (!HasPrevious)
                    throw new InvalidOperationException("There is no previous character, please use HasPrevious to check for this!");
                return sourceCached[sourceOffset - 1];
            }
        }
        /// <summary>
        /// Returns true if there exists a character after the current one, false otherwise.
        /// </summary>
        private bool HasNext => sourceCached != null && sourceOffset <= sourceCached.Length - 1;
        /// <summary>
        /// Returns the character after the current one if it exists.
        /// </summary>
        private char Next
        {
            get
            {
                if (sourceCached == null)
                    throw new InvalidOperationException("This lexer has not been started, cannot access next character!");
                if (!HasNext)
                    throw new InvalidOperationException("There is no next character, please use HasNext to check for this!");
                return sourceCached[sourceOffset + 1];
            }
        }

        /// <summary>
        /// Consumes the current character, moving to the next.
        /// This will also simulate cursor movement in a text document.
        /// As characters are consumed, the "cursor" moves to the right.
        /// The cursor's column increments as it moves, until a line feed is hit.
        /// When the cursor advances past a line feed, the line is incremented and the column is reset to 0.
        /// </summary>
        private void Advance()
        {
            // Store the current character (soon to be the previous one) so we can handle "cursor" movement.
            var previous = Current;
            // Advance the offset.
            sourceOffset++;
            // If advancing the offset pushed us over the edge, then our work is done.
            if (IsEndOfSource)
                return;
            // If the last character was a line feed, this character starts a new line.
            if (previous == '\n')
            {
                line++;
                column = 0;
            }
            // If it WASN'T a line feed, just move the "cursor" over a column.
            else column++;
        }

        private void Advance(int count)
        {
            for (var i = 0; i < count; i++)
                Advance();
        }

        private bool Check(char c) =>
            !IsEndOfSource && Current == c;

        private bool Expect(char c)
        {
            if (!Check(c))
                return false;
            Advance();
            return true;
        }
    }
}
