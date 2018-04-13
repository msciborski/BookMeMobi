using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Headers;

namespace BookMeMobi2.MobiMetadata
{
    public static class MobiService
    {
        public static async Task<MobiDocument> LoadDocument(Stream stream)
        {

            var document = new MobiDocument();
            document.PdbHeader = new PdbHeader(stream);
            await document.PdbHeader.LoadPdbHeader();
            document.MobiHeader = new MobiHeader(stream, document.PdbHeader.MobiHeaderSize);
            await document.MobiHeader.LoadMobiHeader();

            CoverExtractor coverExtractor = 
            document.CoverExtractor = new CoverExtractor(stream);
            return document;
        }
    }
}
