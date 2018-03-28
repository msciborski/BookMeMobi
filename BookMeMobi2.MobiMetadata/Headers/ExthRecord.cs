﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class ExthRecord
    {
        private readonly Stream _stream;
        #region Byte arrays

        private byte[] _type = new byte[4];
        private byte[] _length = new byte[4];


        #endregion

        #region Properties
        public int Type => ByteUtils.GetInt32(_type);

        public int Length => ByteUtils.GetInt32(_length);

        public int Size => GetSize();

        public byte[] Data { get; set; }

        public string Value => ByteUtils.ToString(Data);

        #endregion


        internal ExthRecord(Stream stream)
        {
            _stream = stream;

            //LoadExthRecords();
        }

        internal async Task LoadExthRecords()
        {
            await _stream.ReadAsync(_type, 0, _type.Length);
            await _stream.ReadAsync(_length, 0, _length.Length);

            Data = new byte[Length - 8];

            await _stream.ReadAsync(Data, 0, Data.Length);
        }

        private int GetSize()
        {
            return Data.Length + 8;
        }
    }
}
