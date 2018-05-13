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
        public static async Task<MobiDocument> GetMobiDocument(Stream stream)
        {
            var mobiDocument = new MobiDocument(stream);
            mobiDocument.PDBHeader = new PDBHeader(stream);
            await mobiDocument.PDBHeader.LoadPDBHeader();

            mobiDocument.MOBIHeader = new MOBIHeader(stream, mobiDocument.PDBHeader.MobiHeaderSize);
            await mobiDocument.MOBIHeader.LoadMobiHeader();

            stream.Position = 0;
            mobiDocument.CoverExtractor = new CoverExtractor(stream);
            return mobiDocument;
        }

        public static async Task<MemoryStream> SaveMobiDocument(MobiDocument document)
        {
            MemoryStream memoryStream = new MemoryStream();
            await document.Write(memoryStream);
            return memoryStream;
        }
    }
}
