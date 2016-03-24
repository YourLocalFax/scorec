using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreC
{
    static class CharacterExt
    {
        // TODO(kai): refactor this, it's not great

        public static bool IsDigitInRadix(this char c, int radix)
        {
            if (radix < 2)
                throw new ArgumentException("Radix must be >= 2.");
            if (radix > 36)
                throw new ArgumentException("Radix must be <= 36.");

            c = char.ToLower(c);

            if (c > 'z' || c < '0')
                return false;
            else if (c > '9' && c < 'a')
                return false;

            if (c <= '9')
                return (c - '0') < radix;
            else return (c - 'a' + 10) < radix;
        }
    }
}
