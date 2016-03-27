namespace ScoreC.Compile.Source
{
    using static Source.OperatorKind;

    enum OperatorKind
    {
        /// <summary>
        /// Not an operator.
        /// </summary>
        None,

        Plus,
        PlusEqual,
        PlusPlus,
        PlusPlusEqual,
        Minus,
        MinusEqual,
        MinusMinus,
        MinusMinusEqual,
        MinusGreater,
        Star,
        StarEqual,
        Slash,
        SlashEqual,
        SlashSlash,
        SlashSlashEqual,
        Percent,
        PercentEqual,
        Less,
        LessEqual,
        LessLess,
        LessLessEqual,
        LessLessLess,
        LessLessLessEqual,
        LessGreater,
        LessGreaterEqual,
        Greater,
        GreaterEqual,
        GreaterGreater,
        GreaterGreaterEqual,
        GreaterGreaterGreater,
        GreaterGreaterGreaterEqual,
        Amp,
        AmpEqual,
        AmpAmp,
        AmpAmpEqual,
        Pipe,
        PipeEqual,
        PipePipe,
        PipePipeEqual,
        Caret,
        CaretEqual,
        CaretCaret,
        CaretCaretEqual,
        Tilde,
        TildeEqual,
        Colon,
        ColonColon,
        ColonColonEqual,
        Equal,
        EqualEqual,
        Bang,
        BangEqual,
        BangLessGreater,
        BangLessGreaterEqual,
        BangLess,
        BangLessEqual,
        BangGreater,
        BangGreaterEqual,
        DotDot,
        QuestionQuestion,
    }

    static class OperatorKindExt
    {
        public static int Precedence(this OperatorKind op)
        {
            // FIXME(kai)
            switch (op)
            {
            case AmpAmp:
            case Amp:
            case BangEqual:
            case BangLessGreater:
            case BangLessGreaterEqual:
            case BangLess:
            case BangLessEqual:
            case BangGreater:
            case BangGreaterEqual:
            case CaretCaret:
            case Caret:
            case ColonColon:
            case EqualEqual:
            case Greater:
            case GreaterEqual:
            case GreaterGreater:
            case GreaterGreaterGreater:
            case Less:
            case LessEqual:
            case LessLess:
            case LessLessLess:
            case Minus:
            case MinusMinus:
            case Percent:
            case Pipe:
            case PipePipe:
            case Plus:
            case PlusPlus:
            case QuestionQuestion:
            case Slash:
            case SlashSlash:
            case Star:
            case Tilde:
                return 0;
            default: return -1;
            }
        }

        public static bool IsAssignment(this OperatorKind op)
        {
            switch (op)
            {
            case AmpAmpEqual:
            case AmpEqual:
            case CaretCaretEqual:
            case CaretEqual:
            case ColonColonEqual:
            case Equal:
            case GreaterGreaterEqual:
            case GreaterGreaterGreaterEqual:
            case LessLessEqual:
            case LessLessLessEqual:
            case MinusEqual:
            case MinusMinusEqual:
            case PercentEqual:
            case PipeEqual:
            case PipePipeEqual:
            case PlusEqual:
            case PlusPlusEqual:
            case SlashEqual:
            case SlashSlashEqual:
            case StarEqual:
            case TildeEqual:
                return true;
            default: return false;
            }
        }

        public static bool IsBinary(this OperatorKind op)
        {
            switch (op)
            {
            case AmpAmp:
            case Amp:
            case BangEqual:
            case BangLessGreater:
            case BangLessGreaterEqual:
            case BangLess:
            case BangLessEqual:
            case BangGreater:
            case BangGreaterEqual:
            case CaretCaret:
            case Caret:
            case ColonColon:
            case EqualEqual:
            case Greater:
            case GreaterEqual:
            case GreaterGreater:
            case GreaterGreaterGreater:
            case Less:
            case LessEqual:
            case LessLess:
            case LessLessLess:
            case Minus:
            case MinusMinus:
            case Percent:
            case Pipe:
            case PipePipe:
            case Plus:
            case PlusPlus:
            case QuestionQuestion:
            case Slash:
            case SlashSlash:
            case Star:
            case Tilde:
                return true;
            default: return false;
            }
        }

        public static bool IsUnaryPrefix(this OperatorKind op)
        {
            switch (op)
            {
            case Amp:
            case Bang:
            case Caret:
            case Minus:
            case Star:
            case Tilde:
                return true;
            default: return false;
            }
        }
    }
}
