namespace ScoreC.Compile.Source
{
    using System.Diagnostics;
    using static InfixOperator;

    enum InfixOperator
    {
        /// <summary>
        /// Not an operator.
        /// </summary>
        None,

        // general comparison
        Equal,
        /// <summary>
        /// Also applies to floating point "Unorderd, Less or Greater"
        /// </summary>
        NotEqual,

        // general numeric comparison
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        LessGreater,
        LessGreaterEqual,

        // floating point comparison
        // UnorderedLessGreater (!=)
        Unordered,
        UnorderedEqual,
        UnorderedGreater,
        UnorderedGreaterEqual,
        UnorderedLess,
        UnorderedLessEqual,

        // general numeric arithmetic
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,

        AddEqual,
        SubtractEqual,
        MultiplyEqual,
        DivideEqual,
        ModuloEqual,

        // floating point arithmetic
        IntegerDivide,

        IntegerDivideEqual,

        // bitwise arithmetic
        LeftShift,
        RightShift,
        And,
        Or,
        Xor,

        LeftShiftEqual,
        RightShiftEqual,
        AndEqual,
        OrEqual,
        XorEqual,

        // boolean logic
        LogicAnd,
        LogicOr,
        LogicXor,
    }

    enum PrefixOperator
    {
        // general numeric arithmetic
        Negate,

        // bitwise arithmetic
        Complement,

        // boolean logic
        Not,

        // memory
        Dereference,
        AddressOf,
    }

    static class InfixOperatorExt
    {
        private const int CAST           = 7;
        private const int MULTIPLICITIVE = 6;
        private const int ADDITIVE       = 5;
        private const int BITWISE        = 4;
        private const int SHIFT          = 3;
        private const int RELATIONAL     = 2;
        private const int LOGICAL        = 1;

        public static int Precedence(this InfixOperator op)
        {
            switch (op)
            {
            case Equal:
            case NotEqual:

            case Greater:
            case GreaterEqual:
            case Less:
            case LessEqual:
            case LessGreater:
            case LessGreaterEqual:

            case Unordered:
            case UnorderedEqual:
            case UnorderedGreater:
            case UnorderedGreaterEqual:
            case UnorderedLess:
            case UnorderedLessEqual:
                return RELATIONAL;

            case Add:
            case Subtract:
                return ADDITIVE;
            case Multiply:
            case Divide:
            case Modulo:

            case IntegerDivide:
                return MULTIPLICITIVE;

            case LeftShift:
            case RightShift:
                return SHIFT;
            case And:
            case Or:
            case Xor:
                return BITWISE;

            case LogicAnd:
            case LogicOr:
            case LogicXor:
                return LOGICAL;

            default: return -1;
            }
        }

        public static bool IsAssignment(this InfixOperator op)
        {
            switch (op)
            {
            case AddEqual:
            case SubtractEqual:
            case MultiplyEqual:
            case DivideEqual:
            case ModuloEqual:

            case IntegerDivideEqual:

            case LeftShiftEqual:
            case RightShiftEqual:
            case AndEqual:
            case OrEqual:
            case XorEqual:

                return true;

            default: return false;
            }
        }

        public static InfixOperator AssignmentToInfix(this InfixOperator op)
        {
#if DEBUG
            Debug.Assert(op.IsAssignment());
#endif
            switch (op)
            {
            case AddEqual:
                return Add;
            case SubtractEqual:
                return Subtract;
            case MultiplyEqual:
                return Multiply;
            case DivideEqual:
                return Divide;
            case ModuloEqual:
                return Modulo;

            case IntegerDivideEqual:
                return IntegerDivide;

            case LeftShiftEqual:
                return LeftShift;
            case RightShiftEqual:
                return RightShift;
            case AndEqual:
                return And;
            case OrEqual:
                return Or;
            case XorEqual:
                return Xor;

            default: return None;
            }
        }
    }
}
