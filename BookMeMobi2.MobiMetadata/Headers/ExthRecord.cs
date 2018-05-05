using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class EXTHRecord
    {
        private Stream _stream;

        #region ByteArrays

        private byte[] _recordType = new byte[4];
        private byte[] _recordLength = new byte[4];
        private byte[] _data;

        #endregion

        #region Properties

        public int Size => _data.Length + 8;
        public int RecordType => StreamUtils.ToInt32(_recordType);
        public int RecordLength => StreamUtils.ToInt32(_recordLength);
        public string Value => StreamUtils.ToString(_data);

        #endregion

        public EXTHRecord(Stream stream)
        {
            _stream = stream;
        }

        public EXTHRecord(int type, string data)
        {
            var typeBytes = BitConverter.GetBytes(type);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            var length = data?.Length ?? 0;
            length = length + 8;
            var lengthBytes = BitConverter.GetBytes(length);

            _data = dataBytes;
            _recordType = typeBytes;
            _recordLength = lengthBytes;

        }

        public void SetData(byte[] data)
        {
            _data = data;
            _recordLength = StreamUtils.IntToBytes(data.Length + 8);
        }

        public async Task LoadEXTHRecord()
        {
            await _stream.ReadBytesFromStreamAsync(_recordType);
            await _stream.ReadBytesFromStreamAsync(_recordLength);

            _data = new byte[RecordLength - 8];
            await _stream.ReadBytesFromStreamAsync(_data);
        }

        public async Task Write(Stream stream)
        {
            await stream.WriteAsync(_recordType, 0, _recordType.Length);
            await stream.WriteAsync(_recordLength, 0, _recordLength.Length);
            await stream.WriteAsync(_data, 0, _data.Length);
        }
    }
}
