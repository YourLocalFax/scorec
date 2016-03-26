namespace ScoreC.Compile.Source
{
    enum TokenKind
    {
        None,

        Identifier,
        Keyword,
        WildCard,
        Varargs,
        Uninitialized,

        OpenBracket,
        CloseBracket,
        OpenSquareBracket,
        CloseSquareBracket,
        OpenCurlyBracket,
        CloseCurlyBracket,
        Comma,
        Dot,

        Integer,
        Real,
        String,

        Operator,
        Directive,
    }
}
