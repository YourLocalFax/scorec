using System.Collections.Generic;
using System.Diagnostics;

namespace ScoreC.Compile.Source
{
    using static InfixOperator;
    using static PrefixOperator;

    static class Operator
    {
        private static readonly Dictionary<string, InfixOperator> infixOperatorMap = new Dictionary<string, InfixOperator>()
        {
            // general comparison
            { "==", Equal },
            { "!=", NotEqual },
            
            // general numeric comparison
            { ">", Greater },
            { ">=", GreaterEqual },
            { "<", Less },
            { "<=", LessEqual },
            { "<>", LessGreater },
            { "<>=", LessGreaterEqual },
            
            // floating point comparison
            { "!<>=", Unordered },
            { "!<>", UnorderedEqual },
            { "!<=", UnorderedGreater },
            { "!<", UnorderedGreaterEqual },
            { "!>=", UnorderedLess },
            { "!>", UnorderedLessEqual },

            // general numeric arithmetic
            { "+", Add },
            { "-", Subtract },
            { "*", Multiply },
            { "/", Divide },
            { "%", Modulo },

            { "+=", AddEqual },
            { "-=", SubtractEqual },
            { "*=", MultiplyEqual },
            { "/=", DivideEqual },
            { "%=", ModuloEqual },

            // floating point arithmetic
            { @"\", IntegerDivide },

            { @"\=", IntegerDivideEqual },

            // bitwise arithmetic
            { "<<", LeftShift },
            { ">>", RightShift },
            { "&", And },
            { "|", Or },
            { "~", Xor },

            // boolean logic
            { "&&", LogicAnd },
            { "||", LogicOr },
            { "~~", LogicXor },
        };

        private static readonly Dictionary<string, PrefixOperator> prefixOperatorMap = new Dictionary<string, PrefixOperator>()
        {
            // general numeric arithmetic
            { "-", Negate },
            
            // bitwise arithmetic
            { "~", Complement },
            
            // boolean logic
            { "!", Not },
            
            // memory
            { "*", Dereference },
            { "^", AddressOf },
        };

        public static bool IsInfix(string op) =>
            infixOperatorMap.ContainsKey(op);

        public static bool IsPrefix(string op) =>
            prefixOperatorMap.ContainsKey(op);

        public static InfixOperator GetInfix(string op)
        {
#if DEBUG
            Debug.Assert(IsInfix(op), "`" + op + "` is not a valid Score infix operator!");
#endif
            return infixOperatorMap[op];
        }

        public static int GetPrecedence(string op) =>
            GetInfix(op).Precedence();

        public static PrefixOperator GetPrefix(string op)
        {
#if DEBUG
            Debug.Assert(IsPrefix(op), "`" + op + "` is not a valid Score prefix operator!");
#endif
            return prefixOperatorMap[op];
        }
    }
}
