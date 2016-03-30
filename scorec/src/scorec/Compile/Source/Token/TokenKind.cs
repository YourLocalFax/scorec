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
        Colon,
        Dot,

        Integer,
        Real,
        String,

        Operator,
        Directive,

        Assign,
        GoesTo,
        RangeOf,

        #region Keywords
        // literal
        True,
        False,
        // operator
        Is,
        As,
        Auto,
        // declaration
        Proc,
        Type,
        Data,
        Enum,
        Class,
        Trait,
        Impl,
        Mod,
        Var,
        Let,
        Extern,
        Export,
        // modifier
        Lazy,
        Foreign,
        Sealed,
        Partial,
        Pub,
        Priv,
        Intern,
        // branch.single
        Return,
        Break,
        Continue,
        Goto,
        Resume,
        Yield,
        // branch.multiple
        If,
        Else,
        Unless,
        When,
        Match,
        // branch.loop
        While,
        Until,
        Loop,
        For,
        Each,
        In,
        #endregion
    }
}
