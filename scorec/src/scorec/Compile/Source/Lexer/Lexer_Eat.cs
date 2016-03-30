using ScoreC.Compile.Logging;
using System.Diagnostics;

namespace ScoreC.Compile.Source
{
    sealed partial class Lexer
    {
        /// <summary>
        /// Consumes all white space characters in a row.
        /// </summary>
        private void EatWhiteSpace()
        {
            // Simply eat white space while there are white space characters to eat.
            while (!IsEndOfSource && char.IsWhiteSpace(Current))
                Advance();
        }

        // TODO(kai): Eventually store comments somewhere so that we can access them plz. Great for syntax hilighting.

        /// <summary>
        /// Consumes a line comment + a possible trailing line feed character.
        /// </summary>
        private void EatLineComment()
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource && HasNext, "Not enough characters exist to start a line comment!");
            Debug.Assert(Current == '/' && Next == '#', "Line comment delimiter not found!");
#endif
            // TODO(kai): var start = GetSpan();
            // Eat characters until we hit the end of the line or the end of the source file.
            while (!IsEndOfSource && Current != '\n')
                Advance();
        }

        /// <summary>
        /// Consumes a block comment.
        /// </summary>
        private void EatBlockComment()
        {
#if DEBUG
            Debug.Assert(!IsEndOfSource && HasNext, "Not enough characters exist to start a block comment!");
            Debug.Assert(Current == '/' && Next == '*', "Block comment delimiter not found!");
#endif
            // TODO(kai): Do we want Score to support nested block comments?

            var start = GetSpan();

            // Eat the opening delimiter
            Advance(); // '/'
            Advance(); // '*'

            while (!IsEndOfSource)
            {
                Advance();
                // If the previous character (the one we JUST advanced) was a start, and the current is
                //  a slash, then we've finished the block comment!
                if (!IsEndOfSource && Previous == '*' && Current == '/')
                {
                    Advance();
                    // We return, because falling out of this loop will signal a malformed comment.
                    return;
                }
            }

            Log.AddError(GetSpan(start), "Unfinished block comment, found end of source.");
        }
    }
}
