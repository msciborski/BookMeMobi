using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class ExthHeader
    {
        private readonly Stream _stream;

        #region Byte arrays

        private byte[] _id = new byte[4];
        private byte[] _headerLength = new byte[4];
        private byte[] _recordCount = new byte[4];

        #endregion

        #region Properties

        public string Id => ByteUtils.ToString(_id);
        public int HeaderLength => ByteUtils.GetInt32(_headerLength);
        public int RecordCount => ByteUtils.GetInt32(_recordCount);

        public List<ExthRecord> Records { get; set; }
        public int Size => GetSize();

        #endregion

        internal ExthHeader(Stream stream)
        {
            _stream = stream;
        }

        internal async Task LoadExthHeader()
        {
            await _stream.ReadAsync(_id, 0, _id.Length);
            if (Id != "EXTH")
                throw new ApplicationException("EXTHHeader is invalid.");

            await _stream.ReadAsync(_headerLength, 0, _headerLength.Length);
            await _stream.ReadAsync(_recordCount, 0, _recordCount.Length);

            Records = new List<ExthRecord>();

            for (int i = 0; i < RecordCount; i++)
            {
                var record = new ExthRecord(_stream);
                await record.LoadExthRecords();
                Records.Add(record);
            }

            var padding = new byte[GetPaddingSize(GetDataSize())];
            await _stream.ReadAsync(padding, 0, padding.Length);
        }

        private int GetSize()
        {
            int dataSize = GetDataSize();
            int paddingSize = GetPaddingSize(dataSize);

            return 12 + dataSize + paddingSize;
        }

        private int GetDataSize()
        {
            int dataSize = 0;
            foreach (var record in Records)
            {
                dataSize += record.Size;
            }
            return dataSize;
        }
        private int GetPaddingSize(int dataSize)
        {
            int paddingSize = dataSize % 4;
            if (paddingSize != 0) paddingSize = 4 - paddingSize;

            return paddingSize;
        }
    }
}
