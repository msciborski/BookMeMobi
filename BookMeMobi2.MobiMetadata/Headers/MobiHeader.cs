using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class MOBIHeader
    {
        private Stream _stream;
        private long _mobiHeaderSize;
        private EXTHHeader _exthHeader;

        #region ByteArrays

        private byte[] _compression = new byte[2];
        private byte[] _textLength = new byte[4];
        private byte[] _recordCount = new byte[2];
        private byte[] _recordSize = new byte[2];
        private byte[] _encryptionType = new byte[2];
        private byte[] _headerLength = new byte[4];
        private byte[] _mobiType = new byte[4];
        private byte[] _textEncoding = new byte[4];
        private byte[] _uniqueId = new byte[4];
        private byte[] _fileVersion = new byte[4];
        private byte[] _fullNameOffset = new byte[4];
        private byte[] _fullNameLength = new byte[4];
        private byte[] _locale = new byte[4];
        private byte[] _inputLanguage = new byte[4];
        private byte[] _outputLanguage = new byte[4];
        private byte[] _minMobiPocketVersion = new byte[4];
        private byte[] _firstImageIndex = new byte[4];

        private byte[] _exthFlags = new byte[4];
        private byte[] _fullName;


        //Without properties
        private byte[] _unused0 = new byte[2];
        private byte[] _unused1 = new byte[2];
        private byte[] _mobiIdentifier = new byte[4];
        private byte[] _extraOrtographicnamesKesyFirstNonBookIndex = new byte[44];
        private byte[] _huffmanRecordTableInformation = new byte[16];
        private byte[] _restOfTheMobiHeader;
        private byte[] _remainder;


        #endregion

        #region Properties

        public string Compression => GetCompression();
        public int TextLength => StreamUtils.ToInt32No0Bytes(_textLength);
        public int RecordCount => StreamUtils.ToInt16(_recordCount);
        public int RecordSize => StreamUtils.ToInt16(_recordSize);
        public string EncryptionType => GetEncryptionType();
        public int HeaderLength => StreamUtils.ToInt32(_headerLength);
        public string MobiType => GetMobiType();
        public string TextEncoding => GetTextEncoding();
        public int UniqueId => StreamUtils.ToInt32(_uniqueId);
        public int FileVersion => StreamUtils.ToInt32(_fileVersion);
        public int Locale => StreamUtils.ToInt32No0Bytes(_locale);
        public int InputLanguage => StreamUtils.ToInt32No0Bytes(_inputLanguage);
        public int OutputLanguage => StreamUtils.ToInt32No0Bytes(_outputLanguage);
        public int MinMobiPocketVersion => StreamUtils.ToInt32No0Bytes(_minMobiPocketVersion);
        public int FirstImageIndex => StreamUtils.ToInt32No0Bytes(_firstImageIndex);
        public string FullName => StreamUtils.ToString(_fullName);
        public EXTHHeader EXTHHeader => _exthHeader;

        #endregion


        public MOBIHeader(Stream stream, long mobiHeaderSize)
        {
            _mobiHeaderSize = mobiHeaderSize;
            _stream = stream;
        }

        //Skip means, that i read it from stream, but not exposing it outside this class (reading only for writing purpose) 
        public async Task LoadMobiHeader()
        {
            Console.WriteLine($"MOBI Header start: {_stream.Position}");
            await _stream.ReadBytesFromStreamAsync(_compression);

            //Skip 2 bytes for unused0
            await _stream.ReadBytesFromStreamAsync(_unused0);

            await _stream.ReadBytesFromStreamAsync(_textLength);
            await _stream.ReadBytesFromStreamAsync(_recordCount);
            await _stream.ReadBytesFromStreamAsync(_recordSize);
            await _stream.ReadBytesFromStreamAsync(_encryptionType);

            //Skip 2 bytes for unused1
            await _stream.ReadBytesFromStreamAsync(_unused1);
            //Skip 4 bytes for identifier (characters M O B I)
            await _stream.ReadBytesFromStreamAsync(_mobiIdentifier);

            await _stream.ReadBytesFromStreamAsync(_headerLength);
            await _stream.ReadBytesFromStreamAsync(_mobiType);
            await _stream.ReadBytesFromStreamAsync(_textEncoding);
            await _stream.ReadBytesFromStreamAsync(_uniqueId);
            await _stream.ReadBytesFromStreamAsync(_fileVersion);

            //Skip 44 bytes (Ortographic index, inflection index, indexNames, indexKeys, _extraIndex(0-5), firstNonBookIndex)
            await _stream.ReadBytesFromStreamAsync(_extraOrtographicnamesKesyFirstNonBookIndex);
            Console.WriteLine($"Skip data cursor:{_stream.Position}");
            await _stream.ReadBytesFromStreamAsync(_fullNameOffset);
            await _stream.ReadBytesFromStreamAsync(_fullNameLength);
            await _stream.ReadBytesFromStreamAsync(_locale);
            await _stream.ReadBytesFromStreamAsync(_inputLanguage);
            await _stream.ReadBytesFromStreamAsync(_outputLanguage);
            await _stream.ReadBytesFromStreamAsync(_minMobiPocketVersion);
            await _stream.ReadBytesFromStreamAsync(_firstImageIndex);

            //Skip 16 bytes (Huffman Record Offset, Huffman Record Count, Huffman Table Offset, Huffman Table Length)
            await _stream.ReadBytesFromStreamAsync(_huffmanRecordTableInformation);

            await _stream.ReadBytesFromStreamAsync(_exthFlags);

            //Skip bytes with data, that i dont need, and noone need
            _restOfTheMobiHeader = new byte[(HeaderLength + 16) - 132];
            await _stream.ReadBytesFromStreamAsync(_restOfTheMobiHeader);
            Console.WriteLine($"Cursor position (restOfTheMobiHeader): {_stream.Position}");

            await GetExthHeader();

            int currentOffset = 132 + _restOfTheMobiHeader.Length + EXTHHeader.Size;

            _remainder = new byte[_mobiHeaderSize - currentOffset];
            await _stream.ReadBytesFromStreamAsync(_remainder);

            int fullNameIndexInRemainder = StreamUtils.ToInt32(_fullNameOffset) - currentOffset;
            int fullNameLength = StreamUtils.ToInt32(_fullNameLength);
            _fullName = new byte[fullNameLength];

            if ((fullNameIndexInRemainder >= 0) &&
                    ((fullNameIndexInRemainder + fullNameLength) <= _remainder.Length) &&
                            fullNameLength > 0)
            {
                Array.Copy(_remainder, fullNameIndexInRemainder, _fullName, 0, fullNameLength);
            }
            Console.WriteLine($"MOBI Header end: {_stream.Position}");

        }

        public async Task Write(Stream stream)
        {
            await stream.WriteAsync(_compression, 0, _compression.Length);
            await stream.WriteAsync(_unused0, 0, _unused0.Length);
            await stream.WriteAsync(_textLength, 0, _textLength.Length);
            await stream.WriteAsync(_recordCount, 0, _recordCount.Length);
            await stream.WriteAsync(_recordSize, 0, _recordSize.Length);
            await stream.WriteAsync(_encryptionType, 0, _encryptionType.Length);
            await stream.WriteAsync(_unused1, 0, _unused1.Length);
            await stream.WriteAsync(_mobiIdentifier, 0, _mobiIdentifier.Length);
            await stream.WriteAsync(_headerLength, 0, _headerLength.Length);
            await stream.WriteAsync(_mobiType, 0, _mobiType.Length);
            await stream.WriteAsync(_textEncoding, 0, _textEncoding.Length);
            await stream.WriteAsync(_uniqueId, 0, _uniqueId.Length);
            await stream.WriteAsync(_fileVersion, 0, _fileVersion.Length);
            await stream.WriteAsync(_extraOrtographicnamesKesyFirstNonBookIndex, 0,
                _extraOrtographicnamesKesyFirstNonBookIndex.Length);

            var fullNameOffset = 132 + _restOfTheMobiHeader.Length + EXTHHeader.Size;
            var fullNameOffsetBytes = StreamUtils.IntToBytes(fullNameOffset);

            _fullNameOffset = fullNameOffsetBytes;

            await stream.WriteAsync(_fullNameOffset, 0, _fullNameOffset.Length);
            await stream.WriteAsync(_fullNameLength, 0, _fullNameLength.Length);
            await stream.WriteAsync(_locale, 0, _locale.Length);
            await stream.WriteAsync(_inputLanguage, 0, _inputLanguage.Length);
            await stream.WriteAsync(_outputLanguage, 0, _outputLanguage.Length);
            await stream.WriteAsync(_minMobiPocketVersion, 0, _minMobiPocketVersion.Length);
            await stream.WriteAsync(_firstImageIndex, 0, _firstImageIndex.Length);
            await stream.WriteAsync(_huffmanRecordTableInformation, 0, _huffmanRecordTableInformation.Length);
            await stream.WriteAsync(_exthFlags, 0, _exthFlags.Length);
            await stream.WriteAsync(_restOfTheMobiHeader, 0, _restOfTheMobiHeader.Length);

            if (HasExthHeader())
            {
                await EXTHHeader.Write(stream);
            }

            await stream.WriteAsync(_remainder, 0, _remainder.Length);
        }
        public void SetFullName(string fullName)
        {
            byte[] fullNameBytes = Encoding.UTF8.GetBytes(fullName);

            int newFullNameLength = fullNameBytes.Length;
            byte[] fullNameLengthBytes = StreamUtils.IntToBytes(newFullNameLength);
            _fullNameLength = fullNameLengthBytes;

            int padding = (newFullNameLength + 2) % 4;
            if (padding != 0)
                padding = 4 - padding;
            padding += 2;

            byte[] buffer = new byte[newFullNameLength + padding];
            Array.Copy(fullNameBytes, 0, buffer, 0, newFullNameLength);

            for (int i = newFullNameLength; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }

            _fullName = buffer;

            _remainder = new byte[_fullName.Length];
            Array.Copy(_fullName, 0, _remainder, 0, _remainder.Length);

            var fullNameOffset = 132 + _restOfTheMobiHeader.Length + EXTHHeader.Size;
            var fullNameOffsetBytes = StreamUtils.IntToBytes(fullNameOffset);

            _fullNameOffset = fullNameOffsetBytes;
        }

        private string GetCompression()
        {
            var compression = StreamUtils.ToInt16(_compression);
            switch (compression)
            {
                case 1:
                    return "No compression";
                case 2:
                    return "PalmDOC compression";
                case 17480:
                    return "HUFF/CDIC compression";
            }

            return "";
        }

        private string GetEncryptionType()
        {
            var mobiTypeValue = StreamUtils.ToInt16(_encryptionType);
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

        private string GetMobiType()
        {
            var mobiTypeValue = StreamUtils.ToInt32(_mobiType);
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
            var textEncodingValue = StreamUtils.ToInt32(_textEncoding);
            switch (textEncodingValue)
            {
                case 1252:
                    return "Cp1252";
                case 65001:
                    return "UTF-8";
            }

            return "";
        }

        private bool HasExthHeader()
        {
            var bytes = (byte[])_exthFlags.Clone();

            //if(BitConverter.IsLittleEndian)
            //    Array.Reverse(bytes);

            int exthFlagsValue = StreamUtils.ToInt32No0Bytes(bytes);

            return (exthFlagsValue & 0x40) != 0;
        }

        private async Task GetExthHeader()
        {
            if (HasExthHeader())
            {
                _exthHeader = new EXTHHeader(_stream);
                await _exthHeader.LoadEXTHHeader();
            }
        }
    }
}
