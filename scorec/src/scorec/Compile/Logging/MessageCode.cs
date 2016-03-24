using System.Globalization;

namespace ScoreC.Compile.Logging
{
    enum MessageCode : ushort
    {
        #region Lex Stage Errors
        TokenLexFailed = 0x0001,
        InvalidDigitInRadix = 0x0002,
        InvalidDigitSeparatorLocation = 0x0003,
        UnexpectedEndOfSource = 0x0004,
        UnfinishedLiteral = 0x0005,
        InvalidOperatorToken = 0x0006,
        #endregion

        #region Parse Stage Errors
        NodeParseFailed = 0x0001,
        UnnamedProcedure = 0x1002,
        TypeParseFailed = 0x1003,
        MissingProcedureType = 0x1004,
        MissingProcedureBody = 0x1005,
        MissingType = 0x1006,
        ExpressionParseFailed = 0x1007,
        UnexpectedToken = 0x1008,
        #endregion
    }

    static class MessageCodeExt
    {
        public static string ToCodeString(this MessageCode code) =>
            string.Format(CultureInfo.InvariantCulture, "SC{0:X4}", (uint)code);
    }
}
