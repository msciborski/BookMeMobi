using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BookMeMobi2.MobiMetadata.Headers;

namespace BookMeMobi2.MobiMetadata
{
    public static class MobiService
    {
        public static MobiDocument LoadDocument(string filePath)
        {

            var document = new MobiDocument(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                document.PdbHeader = new PdbHeader(fs);
                document.MobiHeader = new MobiHeader(fs, document.PdbHeader.MobiHeaderSize);
            }

            return document;
        }
    }
}
