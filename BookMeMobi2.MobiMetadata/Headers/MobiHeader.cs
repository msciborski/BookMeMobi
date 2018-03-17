﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class MobiHeader
    {
        private readonly FileStream _stream;
        private readonly int _mobiHeaderSize;

        #region Byte arrays

        private byte[] _compression = new byte[2];
        private byte[] _unused0 = new byte[2];
        private byte[] _textLength = new byte[4];
        private byte[] _recordCount = new byte[2];
        private byte[] _recordSize = new byte[2];
        private byte[] _encryptionType = new byte[2];
        private byte[] _unused1 = new byte[2];
        private byte[] _identifier = new byte[4];
        private byte[] _headerLength = new byte[4];
        private byte[] _mobiType = new byte[4];
        private byte[] _textEncoding = new byte[4];
        private byte[] _uniqueId = new byte[4];
        private byte[] _fileVersion = new byte[4];
        private byte[] _orthographicIndex = new byte[4];
        private byte[] _inflectionIndex = new byte[4];
        private byte[] _indexNames = new byte[4];
        private byte[] _indexKeys = new byte[4];
        private byte[] _extraIndex0 = new byte[4];
        private byte[] _extraIndex1 = new byte[4];
        private byte[] _extraIndex2 = new byte[4];
        private byte[] _extraIndex3 = new byte[4];
        private byte[] _extraIndex4 = new byte[4];
        private byte[] _extraIndex5 = new byte[4];
        private byte[] _firstNonBookIndex = new byte[4];
        private byte[] _fullNameOffset = new byte[4];
        private byte[] _fullNameLength = new byte[4];
        private byte[] _locale = new byte[4];
        private byte[] _inputLanguage = new byte[4];
        private byte[] _outputLanguage = new byte[4];
        private byte[] _minVersion = new byte[4];
        private byte[] _firstImageIndex = new byte[4];
        private byte[] _huffmanRecordOffset = new byte[4];
        private byte[] _huffmanRecordCount = new byte[4];
        private byte[] _huffmanTableOffset = new byte[4];
        private byte[] _huffmanTableLength = new byte[4];
        private byte[] _exthFlags = new byte[4];
        private byte[] _restOfMobiHeader = null;
        private byte[] _remainder = null;
        private byte[] _fullname = null;

        #endregion

        #region Properties

        public string Compression => GetCompressionType();
        public int TextLength => ByteUtils.ToInt32(_textLength);
        public short RecordCount => ByteUtils.ToInt16(_recordCount);
        public short RecordSize => ByteUtils.ToInt16(_recordSize);
        public string EncryptionType => GetEncryptionType();
        public string Identifier => ByteUtils.ToString(_identifier);
        public int HeaderLength => ByteUtils.GetInt32(_headerLength);
        public string MobiType => GetMobiType();
        public string TextEncoidng => GetTextEncoding();
        public long UniqueId => ByteUtils.ToUInt32(_uniqueId);
        public long FileVersion => ByteUtils.ToUInt32(_fileVersion);
        public int Locale => ByteUtils.ToInt32(_locale);
        public int InputLanguage => ByteUtils.ToInt32(_inputLanguage);
        public int OutputLanguage => ByteUtils.ToInt32(_outputLanguage);
        public int MinVersion => ByteUtils.ToInt32(_minVersion);
        public int FirstImageIndex => ByteUtils.ToInt32(_firstImageIndex);
        public string FullName => ByteUtils.ToString(_fullname);

        public ExthHeader ExthHeader { get; set; }

        #endregion



        internal MobiHeader(FileStream stream, int mobiHeaderSize)
        {
            _stream = stream;
            _mobiHeaderSize = mobiHeaderSize;

            LoadMobiHeader();
        }

        private void LoadMobiHeader()
        {
            _stream.Read(_compression, 0, _compression.Length);
            _stream.Read(_unused0, 0, _unused0.Length);
            _stream.Read(_textLength, 0, _textLength.Length);
            _stream.Read(_recordCount, 0, _recordCount.Length);
            _stream.Read(_recordSize, 0, _recordSize.Length);
            _stream.Read(_encryptionType, 0, _encryptionType.Length);
            _stream.Read(_unused1, 0, _unused1.Length);
            _stream.Read(_identifier, 0, _identifier.Length);
            _stream.Read(_headerLength, 0, _headerLength.Length);
            _stream.Read(_mobiType, 0, _mobiType.Length);
            _stream.Read(_textEncoding, 0, _textEncoding.Length);
            _stream.Read(_uniqueId, 0, _uniqueId.Length);
            _stream.Read(_fileVersion, 0, _fileVersion.Length);
            _stream.Read(_orthographicIndex, 0, _orthographicIndex.Length);
            _stream.Read(_inflectionIndex, 0, _inflectionIndex.Length);
            _stream.Read(_indexNames, 0, _indexNames.Length);
            _stream.Read(_indexKeys, 0, _indexKeys.Length);
            _stream.Read(_extraIndex0, 0, _extraIndex0.Length);
            _stream.Read(_extraIndex1, 0, _extraIndex1.Length);
            _stream.Read(_extraIndex2, 0, _extraIndex2.Length);
            _stream.Read(_extraIndex3, 0, _extraIndex3.Length);
            _stream.Read(_extraIndex4, 0, _extraIndex4.Length);
            _stream.Read(_extraIndex5, 0, _extraIndex5.Length);
            _stream.Read(_firstNonBookIndex, 0, _firstNonBookIndex.Length);
            _stream.Read(_fullNameOffset, 0, _fullNameOffset.Length);
            _stream.Read(_fullNameLength, 0, _fullNameLength.Length);
            _stream.Read(_locale, 0, _locale.Length);
            _stream.Read(_inputLanguage, 0, _inputLanguage.Length);
            _stream.Read(_outputLanguage, 0, _outputLanguage.Length);
            _stream.Read(_minVersion, 0, _minVersion.Length);
            _stream.Read(_firstImageIndex, 0, _firstImageIndex.Length);
            _stream.Read(_huffmanRecordOffset, 0, _huffmanRecordOffset.Length);
            _stream.Read(_huffmanRecordCount, 0, _huffmanRecordCount.Length);
            _stream.Read(_huffmanTableOffset, 0, _huffmanTableOffset.Length);
            _stream.Read(_huffmanTableLength, 0, _huffmanTableLength.Length);
            _stream.Read(_exthFlags, 0, _exthFlags.Length);

            _restOfMobiHeader = new byte[HeaderLength + 16 - 132];
            _stream.Read(_restOfMobiHeader, 0, _restOfMobiHeader.Length);

            var exthHeaderExists = GetExthHeaderExists();
            if (exthHeaderExists)
            {
                ExthHeader = new ExthHeader(_stream);
            }

            int exthHeaderSize = exthHeaderExists ? ExthHeader.Size : 0;
            int currentOffset = 132 + _restOfMobiHeader.Length + exthHeaderSize;

            _remainder = new byte[_mobiHeaderSize - currentOffset];
            _stream.Read(_remainder, 0, _remainder.Length);

            int fullNameIndexInRemainder = ByteUtils.GetInt32(_fullNameOffset) - currentOffset;
            int fullNameLength = ByteUtils.GetInt32(_fullNameLength);
            _fullname = new byte[fullNameLength];

            if (fullNameIndexInRemainder >= 0
                && (fullNameIndexInRemainder < _remainder.Length)
                && ((fullNameIndexInRemainder + fullNameLength) <= _remainder.Length)
                && (fullNameLength > 0))
            {
                Array.Copy(_remainder, fullNameIndexInRemainder, _fullname, 0, fullNameLength);
            }

        }

        public string GetExthRecordValue(int type)
        {

            if (ExthHeader == null || ExthHeader.Records == null)
                return "";

            var record = ExthHeader.Records.FirstOrDefault(r => r.Type == type);

            if (record == null)
                return "";

            return record.Value;

        }
        private bool GetExthHeaderExists()
        {
            var bytes = (byte[])_exthFlags.Clone();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            int exthFlagsValue = BitConverter.ToInt32(bytes, 0);

            return (exthFlagsValue & 0x40) != 0;
        }
        private string GetCompressionType()
        {
            var compression = ByteUtils.ToInt16(_compression);
            switch (compression)
            {
                case 1:
                    return "No Compression";
                case 2:
                    return "PalmDOC Compression";
                case 3:
                    return "HUFF/CDIC Compression";
            }
            return "";
        }

        private string GetEncryptionType()
        {
            var encType = ByteUtils.ToInt16(_encryptionType);
            switch (encType)
            {
                case 0:
                    return "No Ecryption";
                case 1:
                    return "Old Mobipocket Encryption";
                case 2:
                    return "Mobipocket Encryption";
            }
            return "";
        }
        private string GetMobiType()
        {
            var mobiTypeValue = ByteUtils.ToInt32(_mobiType);
            switch (mobiTypeValue)
            {
                case 2:
                    return "MobiPocket Book";
                case 3:
                    return "PalmDOC Book";
                case 4: return "Audio";
                case 232:
                    return "MobiPocket Generated by Kindlegen 1.2";
                case 248:
                    return "KF8 Generated by Kindlegen 1.2";
                case 257:
                    return "News";
                case 258:
                    return "News Feed";
                case 259:
                    return "News Magazine";
                case 513:
                    return "PICS";
                case 514:
                    return "Word";
                case 515:
                    return "XLS";
                case 516:
                    return "PPT";
                case 517:
                    return "TEXT";
                case 518:
                    return "HTML";
            }
            return "";
        }
        private string GetTextEncoding()
        {
            var textEncodingValue = ByteUtils.ToInt32(_textEncoding);
            switch (textEncodingValue)
            {
                case 1252:
                    return "Cp1252";
                case 65001:
                    return "UTF-8";
            }

            return "";
        }
    }
}
