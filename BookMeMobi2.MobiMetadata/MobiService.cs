using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BookMeMobi2.MobiMetadata.Headers;

namespace BookMeMobi2.MobiMetadata
{
    public static class MobiService
    {
        public static MobiDocument LoadDocument(Stream stream)
        {

            var document = new MobiDocument();
            document.PdbHeader = new PdbHeader(stream);
            document.MobiHeader = new MobiHeader(stream, document.PdbHeader.MobiHeaderSize);

            return document;
        }
    }
}
