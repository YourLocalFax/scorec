using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ScoreC.Compile.Source
{
    using static OperatorKind;

    static class Operator
    {
        private static readonly Dictionary<string, OperatorKind> operatorMap = new Dictionary<string, OperatorKind>()
        {
            { "+",      Plus },
            { "+=",     PlusEqual },
            { "++",     PlusPlus },
            { "++=",    PlusPlusEqual },
            { "-",      Minus },
            { "-=",     MinusEqual },
            { "--",     MinusMinus },
            { "--=",    MinusMinusEqual },
            { "->",     MinusGreater },
            { "*",      Star },
            { "*=",     StarEqual },
            { "/",      Slash },
            { "/=",     SlashEqual },
            { "//",     SlashSlash },
            { "//=",    SlashSlashEqual },
            { "%",      Percent },
            { "%=",     PercentEqual },
            { "<",      Less },
            { "<=",     LessEqual },
            { "<<",     LessLess },
            { "<<=",    LessLessEqual },
            { "<>",     LessGreater },
            { "<>=",    LessGreaterEqual },
            { ">",      Greater },
            { ">=",     GreaterEqual },
            { ">>",     GreaterGreater },
            { ">>=",    GreaterGreaterEqual },
            { "&",      Amp },
            { "&=",     AmpEqual },
            { "&&",     AmpAmp },
            { "&&=",    AmpAmpEqual },
            { "|",      Pipe },
            { "|=",     PipeEqual },
            { "||",     PipePipe },
            { "||=",    PipePipeEqual },
            { "^",      Caret },
            { "^=",     CaretEqual },
            { "^^",     CaretCaret },
            { "^^=",    CaretCaretEqual },
            { "~",      Tilde },
            { "~=",     TildeEqual },
            { ":",      Colon },
            { "::",     ColonColon },
            { "::=",    ColonColonEqual },
            { "=",      Equal },
            { "==",     EqualEqual },
            { "!",      Bang },
            { "!=",     BangEqual },
            { "!<>",    BangLessGreater },
            { "!<>=",   BangLessGreaterEqual },
            { "!<",     BangLess },
            { "!<=",    BangLessEqual },
            { "!>",     BangGreater },
            { "!>=",    BangGreaterEqual },
            { "..",     DotDot },
            { "??",     QuestionQuestion },
        };

        private static readonly string[] operators = operatorMap.Select(pair => pair.Key).ToArray();
        public static string[] Operators => (string[])operators.Clone();

        public static bool IsOperatorStart(char c)
        {
            foreach (var op in operators)
                // NOTE(kai): This should never fail, but it's not great practice anyway (I think, at least).
                if (op[0] == c)
                    return true;
            return false;
        }

        public static OperatorKind GetKindFromOperator(string op)
        {
#if DEBUG
            Debug.Assert(operators.Contains(op), op + " is not a valid Score operator!");
#endif
            return operatorMap[op];
        }
    }
}
