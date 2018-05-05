using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BookMeMobi2.MobiMetadata.Utilities
{
    public static class StreamExtensions
    {
        public static async Task<int> ReadBytesFromStreamAsync(this Stream source, byte[] destinationArray)
        {
            return await source.ReadAsync(destinationArray, 0, destinationArray.Length);
        }
    }
}
