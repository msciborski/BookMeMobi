using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class EXTHHeader
    {
        private Stream _stream;

        #region ByteArrays
        private byte[] _headerLength = new byte[4];
        private byte[] _recordCount = new byte[4];

        private byte[] _identifier = new byte[4];
        private byte[] _padding;

        #endregion

        #region Properties

        public int HeaderLength => StreamUtils.ToInt32(_headerLength);
        public int RecordCount => StreamUtils.ToInt32(_recordCount);
        public int Size => GetSize();
        public List<EXTHRecord> EXTHRecords { get; } = new List<EXTHRecord>();

        #endregion

        public EXTHHeader(Stream stream)
        {
            _stream = stream;
        }

        public async Task LoadEXTHHeader()
        {
            await _stream.ReadBytesFromStreamAsync(_identifier);
            await _stream.ReadBytesFromStreamAsync(_headerLength);
            await _stream.ReadBytesFromStreamAsync(_recordCount);

            await GetEXTHRecords();

            int paddingSize = GetPaddingSize(GetDataSize());
            _padding = new byte[paddingSize];
            await _stream.ReadBytesFromStreamAsync(_padding);

        }

        public async Task Write(Stream stream)
        {
            await stream.WriteAsync(_identifier, 0, _identifier.Length);
            await stream.WriteAsync(_headerLength, 0, _headerLength.Length);
            await stream.WriteAsync(_recordCount, 0, _recordCount.Length);

            foreach (var record in EXTHRecords)
            {
                await record.Write(stream);
            }

            var padding = new byte[GetPaddingSize(GetDataSize())];
            await stream.WriteAsync(padding, 0, padding.Length);
        }

        private int GetSize()
        {
            int dataSize = GetDataSize();
            return 12 + dataSize + GetPaddingSize(dataSize);
        }

        private int GetDataSize()
        {
            return EXTHRecords.Sum(r => r.Size);
        }

        private int GetPaddingSize(int dataSize)
        {
            int paddingSize = dataSize % 4;
            if (paddingSize != 0)
                paddingSize = 4 - paddingSize;
            return paddingSize;
        }

        private async Task GetEXTHRecords()
        {
            for (int i = 0; i < RecordCount; i++)
            {
                var exthRecord = new EXTHRecord(_stream);
                await exthRecord.LoadEXTHRecord();
                EXTHRecords.Add(exthRecord);
            }
        }
        public EXTHRecord GetExthRecord(int type)
        {
            if (EXTHRecords == null)
            {
                return null;
            }

            var record = EXTHRecords.FirstOrDefault(r => r.RecordType == type);
            return record;
        }
        public string GetEXTHRecordValue(int type)
        {
            if (EXTHRecords == null)
                return "";

            var record = EXTHRecords.FirstOrDefault(r => r.RecordType == type);

            if (record == null)
                return "";
            return record.Value;
        }

        public void ModifyExthRecord(int type, byte[] data)
        {
            var record = GetExthRecord(type);
            record.SetData(data);
            RecomputeFields();
        }

        private void RecomputeFields()
        {
            _headerLength = StreamUtils.IntToBytes(Size);
            _recordCount = StreamUtils.IntToBytes(EXTHRecords.Count);
        }
    }
}
