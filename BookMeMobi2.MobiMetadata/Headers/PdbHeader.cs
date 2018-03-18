using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class PdbHeader
    {
        private readonly Stream _stream;

        #region Byte arrays

        public byte[] _name = new byte[32];
        public byte[] _attributes = new byte[2];
        public byte[] _version = new byte[2];
        public byte[] _createDate = new byte[4];
        public byte[] _lastBackupDate = new byte[4];
        public byte[] _modificationDate = new byte[4];
        public byte[] _modificationNumber = new byte[4];
        public byte[] _appInfoId = new byte[4];
        public byte[] _sortInfoId = new byte[4];
        public byte[] _type = new byte[4];
        public byte[] _creator = new byte[4];
        public byte[] _uniqueIdSeed = new byte[4];
        public byte[] _nextRecordListId = new byte[4];
        public byte[] _numberOfRecords = new byte[2];
        public byte[] _gapToData = new byte[2];

        #endregion

        #region Properties

        public string Name => ByteUtils.ToString(_name);
        public int Attributes => ByteUtils.GetInt32(_attributes);
        public int Version => ByteUtils.GetInt32(_version);
        public DateTime? CreateDate => GetHeaderDate(_createDate);
        public DateTime? ModificationDate => GetHeaderDate(_modificationDate);
        public DateTime? LastBackupDate => GetHeaderDate(_lastBackupDate);
        public int ModificationNumber => ByteUtils.GetInt32(_modificationNumber);
        public int AppInfoId => ByteUtils.GetInt32(_appInfoId);
        public int SortInfoId => ByteUtils.GetInt32(_sortInfoId);
        public int Type => ByteUtils.GetInt32(_type);
        public int Creator => ByteUtils.GetInt32(_creator);
        public int UniqueIdSeed => ByteUtils.GetInt32(_uniqueIdSeed);
        public int NextRecordListId => ByteUtils.GetInt32(_nextRecordListId);
        public int NumberOfRecords => ByteUtils.GetInt32(_numberOfRecords);
        public List<PdbRecord> Records { get; set; }
        public int GapToData => ByteUtils.GetInt32(_gapToData);
        public int MobiHeaderSize => Records.Count > 1 ? Records[1].Offset - Records[0].Offset : 0;
        public int OffsetAfterMobiHeader => Records.Count > 1 ? Records[1].Offset : 0;

        #endregion




        internal PdbHeader(Stream stream)
        {
            _stream = stream;

            LoadPdbHeader();

        }

        private void LoadPdbHeader()
        {

            _stream.Read(_name, 0, _name.Length);
            _stream.Read(_attributes, 0, _attributes.Length);
            _stream.Read(_version, 0, _version.Length);
            _stream.Read(_createDate, 0, _createDate.Length);
            _stream.Read(_lastBackupDate, 0, _lastBackupDate.Length);
            _stream.Read(_modificationDate, 0, _modificationDate.Length);
            _stream.Read(_modificationNumber, 0, _modificationNumber.Length);
            _stream.Read(_appInfoId, 0, _appInfoId.Length);
            _stream.Read(_sortInfoId, 0, _sortInfoId.Length);
            _stream.Read(_type, 0, _type.Length);
            _stream.Read(_creator, 0, _creator.Length);
            _stream.Read(_uniqueIdSeed, 0, _uniqueIdSeed.Length);
            _stream.Read(_nextRecordListId, 0, _nextRecordListId.Length);
            _stream.Read(_numberOfRecords, 0, _numberOfRecords.Length);

            Records = new List<PdbRecord>();
            for (var i = 0; i < NumberOfRecords; i++)
            {
                Records.Add(new PdbRecord(_stream));
            }

            _stream.Read(_gapToData, 0, _gapToData.Length);
        }

        private DateTime? GetHeaderDate(byte[] secondBytes)
        {

            int seconds = ByteUtils.GetInt32(secondBytes);

            if (seconds == 0)
                return null;

            var date = new DateTime(1970, 1, 1, 0, 0, 0);
            return date.AddSeconds(seconds);
        }
    }
}
