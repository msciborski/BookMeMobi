using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class PDBRecord
    {
        private Stream _stream;

        #region ByteArrays

        private byte[] _offset = new byte[4];
        private byte[] _uniqueId = new byte[3];
        private byte[] _attributes = new byte[1];

        #endregion

        #region Properties

        public int Offset => StreamUtils.ToInt32(_offset);
        public int UniqueId => StreamUtils.ToInt32(_uniqueId);
        public byte Attributes => _attributes[0];

        #endregion

        public PDBRecord(Stream stream)
        {
            _stream = stream;
        }

        public async Task LoadPDBRecord()
        {
            await _stream.ReadBytesFromStreamAsync(_offset);
            await _stream.ReadBytesFromStreamAsync(_uniqueId);
            await _stream.ReadBytesFromStreamAsync(_attributes);

        }

        public async Task Write(Stream stream)
        {
            await stream.WriteAsync(_offset, 0, _offset.Length);
            await stream.WriteAsync(_attributes, 0, _attributes.Length);
            await stream.WriteAsync(_uniqueId, 0, _uniqueId.Length);
        }
    }
}
