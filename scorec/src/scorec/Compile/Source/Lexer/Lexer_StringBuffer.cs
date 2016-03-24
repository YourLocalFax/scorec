namespace ScoreC.Compile.Source
{
    sealed partial class Lexer
    {
        /// <summary>
        /// Clears the string buffer.
        /// </summary>
        private void ClearBuffer() =>
            buffer.Clear();

        /// <summary>
        /// Appends the current character to the end of the string buffer and advances to the next character.
        /// </summary>
        private void Bump()
        {
            Append(Current);
            Advance();
        }

        /// <summary>
        /// Appends the given character to the end of the string buffer.
        /// </summary>
        private void Append(char c) =>
            buffer.Append(c);

        /// <summary>
        /// Returns the contents of the string buffer as a string.
        /// </summary>
        private string GetStringFromBuffer() =>
            buffer.ToString();
    }
}
