using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScoreC.Compile.SyntaxTree
{
    enum BuiltinType
    {
        VOID,
        BOOL,
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
        INT,
        UINT,
        USIZE,
        PTRDIFF,
        R16,
        R32,
        R64,
        R80,
        R128,
        REAL,
        C8,
        C16,
        C32,
    }

    sealed class BuiltinTypeInfo : TypeInfo
    {
        private static Dictionary<string, BuiltinTypeInfo> builtins = new Dictionary<string, BuiltinTypeInfo>();

        /// <summary>
        /// Attempts to get the BuiltinTypeInfo from the given image.
        /// Will return null if passed a non-builtin type name image.
        /// </summary>
        /// <returns></returns>
        public static BuiltinTypeInfo Get(string image)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(image), "BuiltinTypeInfo.Get needs an image you dumbass.");
            Debug.Assert(image == image.ToLower(), "No builtin types contain uppercase letters, sucks don't it.");
#endif
            BuiltinTypeInfo info;
            if (builtins.TryGetValue(image, out info))
                return info;

            BuiltinType type;
            if (Enum.TryParse(image.ToUpper(), out type))
            {
                info = new BuiltinTypeInfo(image, type);
                builtins[image] = info;
                return info;
            }
            else
            {
#if DEBUG
                Debug.Assert(false, string.Format("This shouldn't happen, dood, you passed a non-builtin type name `{0}` to the builtin getter dood.", image));
#endif
                return null;
            }
        }

        public static BuiltinTypeInfo Get(BuiltinType type) =>
            Get(type.ToString().ToLower());

        /// <summary>
        /// Determines if the given image is a valid builtin type name.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static bool IsValid(string image) =>
            Get(image) != null;

        public string Image;
        public BuiltinType Type;

        private BuiltinTypeInfo(string image, BuiltinType type)
        {
            Image = image;
            Type = type;
        }

        public override string ToString() => Image;
    }
}
