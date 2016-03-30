using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoreC
{
    class utf8
    {
        #region Implicit Conversions
        public static implicit operator utf8(string value) =>
            new utf8(value);

        public static implicit operator string (utf8 value) =>
            value.ToString();
        #endregion

        private byte[] value;

        public int ByteCount => value.Length;
        public long LongByteCount => value.LongLength;

        // NOTE(kai): this is really only used for checking #char directives right now,
        //  but I wanted to implement it fully anyway.
        public int CodePointCount
        {
            get
            {
                int totalCount = 0;

                var bytes = value;
                var byteCount = bytes.Length;
                int offset = 0;

                while (offset < byteCount)
                {
                    if ((bytes[offset] & 0xC0) != 0x80)
                        totalCount += 1;
                    offset += 1;
                }

                return totalCount;
            }
        }

        public utf8(string str)
        {
            var strBytesUnicode = Encoding.Unicode.GetBytes(str);
            value = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, strBytesUnicode);
        }

        public override string ToString() =>
            Encoding.UTF8.GetString(value);
    }
}
