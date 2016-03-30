using System.Collections.Generic;
using System.Diagnostics;

namespace ScoreC.Compile.SyntaxTree
{
    using static BuiltinType;

    enum BuiltinType
    {
        Void,
        Bool,

        S8,
        S16,
        S32,
        S64,
        S128,

        U8,
        U16,
        U32,
        U64,
        U128,

        R16,
        R32,
        R64,
        R80,
        R128,

        Int,
        Uint,
        Usize,
        Ptrdiff,
        Real,
    }

    sealed class BuiltinTypeInfo : TypeInfo
    {
        private static Dictionary<string, BuiltinTypeInfo> builtins = new Dictionary<string, BuiltinTypeInfo>()
        {
            { "void",     new BuiltinTypeInfo(Void)    },
            { "bool",     new BuiltinTypeInfo(Bool)    },

            { "s8",       new BuiltinTypeInfo(S8)      },
            { "s16",      new BuiltinTypeInfo(S16)     },
            { "s32",      new BuiltinTypeInfo(S32)     },
            { "s64",      new BuiltinTypeInfo(S64)     },
            { "s128",     new BuiltinTypeInfo(S128)    },

            { "u8",       new BuiltinTypeInfo(U8)      },
            { "u16",      new BuiltinTypeInfo(U16)     },
            { "u32",      new BuiltinTypeInfo(U32)     },
            { "u64",      new BuiltinTypeInfo(U64)     },
            { "u128",     new BuiltinTypeInfo(U128)    },

            { "r16",      new BuiltinTypeInfo(R16)     },
            { "r32",      new BuiltinTypeInfo(R32)     },
            { "r64",      new BuiltinTypeInfo(R64)     },
            { "r80",      new BuiltinTypeInfo(R80)     },
            { "r128",     new BuiltinTypeInfo(R128)    },

            { "int",      new BuiltinTypeInfo(Int)     },
            { "uint",     new BuiltinTypeInfo(Uint)    },
            { "usize",    new BuiltinTypeInfo(Usize)   },
            { "ptrdiff",  new BuiltinTypeInfo(Ptrdiff) },
            { "real",     new BuiltinTypeInfo(Real)    },
        };

        /// <summary>
        /// Attempts to get the BuiltinTypeInfo from the given image.
        /// Will return null if passed a non-builtin type name image.
        /// </summary>
        /// <returns></returns>
        public static BuiltinTypeInfo Get(string image)
        {
#if DEBUG
            Debug.Assert(IsBulitinTypeName(image), "Invalid builtin type image `" + image + "` passed!");
#endif
            return builtins[image];
        }

        public static BuiltinTypeInfo Get(BuiltinType type)
        {
            foreach (var pair in builtins)
                if (pair.Value.Type == type)
                    return pair.Value;
#if DEBUG
            Debug.Assert(false);
#endif
            return null;
        }

        /// <summary>
        /// Determines if the given image is a valid builtin type name.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static bool IsBulitinTypeName(string image) =>
            builtins.ContainsKey(image);

        public string Image;
        public BuiltinType Type;

        private BuiltinTypeInfo(BuiltinType type)
        {
            Image = type.ToString().ToLower();
            Type = type;
        }

        public override string ToString() => Image;
    }
}
