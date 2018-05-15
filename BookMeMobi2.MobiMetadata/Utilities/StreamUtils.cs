using System;
using System.Collections.Generic;
using System.Text;

namespace BookMeMobi2.MobiMetadata.Utilities
{
    public static class StreamUtils
    {
        public static string ToString(byte[] bytes)
        {
            if (bytes.Length == 0)
                return String.Empty;
            return Encoding.UTF8.GetString(bytes).ReplaceNullTerminatorWithEmpty();
        }

        public static int ToInt16(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public static int ToInt32(byte[] bytes)
        {
            var result = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                result = (result << 8) + (bytes[i] & 0xff);
            }

            return result;
        }

        public static int ToInt32No0Bytes(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static DateTime? ToDateTime(byte[] bytes)
        {
            int seconds = ToInt32(bytes);

            if (seconds == 0)
                return null;

            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(seconds);
        }

        public static byte[] IntToBytes(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
    }
}
