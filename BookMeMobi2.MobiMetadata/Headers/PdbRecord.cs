﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class PdbRecord
    {
        private readonly Stream _stream;

        #region Byte arrays

        private byte[] _offset = new byte[4];
        private byte[] _uniqueId = new byte[3];
        private byte[] _attributes = new byte[1];

        #endregion

        #region Properties

        public int Offset => ByteUtils.GetInt32(_offset);
        public byte Attributes => _attributes[0];
        public int UniqueId => ByteUtils.GetInt32(_uniqueId);

        #endregion

        internal PdbRecord(Stream stream)
        {
            _stream = stream;

            LoadRecordInfo();
        }

        private void LoadRecordInfo()
        {
            _stream.Read(_offset, 0, _offset.Length);
            _stream.Read(_attributes, 0, _attributes.Length);
            _stream.Read(_uniqueId, 0, _uniqueId.Length);
        }
    }
}