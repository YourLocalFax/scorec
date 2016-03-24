namespace ScoreC.Compile.Source
{
    enum Keyword
    {
        // not-a-keyword-oops
        None,
        // literal
        True,
        False,
        // operator
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
    }
}
