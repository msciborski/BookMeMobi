using System;
using System.Collections.Generic;
using System.Text;

namespace BookMeMobi2.MobiMetadata.Utilities
{
    internal static class ByteUtils
    {
        public static short ToInt16(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static int ToInt32(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int GetInt32(byte[] bytes)
        {
            var result = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                result = (result << 8) + (bytes[i] & 0xff);
            }

            return result;
        }

        public static long ToUInt32(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string ToString(byte[] bytes)
        {
            if (bytes == null)
                return "";
            return Encoding.ASCII.GetString(bytes).Replace("\0", "");
        }
    }
}
}
