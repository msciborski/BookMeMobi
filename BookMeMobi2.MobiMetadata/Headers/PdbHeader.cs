using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Utilities;

namespace BookMeMobi2.MobiMetadata.Headers
{
    public class PDBHeader
    {
        private Stream _stream;

        #region ByteArrays

        private byte[] _name = new byte[32];
        private byte[] _attributes = new byte[2];
        private byte[] _version = new byte[2];
        private byte[] _creationDate = new byte[4];
        private byte[] _backupDate = new byte[4];
        private byte[] _modificationDate = new byte[4];
        private byte[] _modificationNumber = new byte[4];
        private byte[] _applicationInfoId = new byte[4];
        private byte[] _applicationSortId = new byte[4];
        private byte[] _type = new byte[4];
        private byte[] _creator = new byte[4];
        private byte[] _uniqeIdSeed = new byte[4];
        private byte[] _nextRecordList = new byte[2];
        private byte[] _recordsCount = new byte[4];

        private byte[] _gapData = new byte[2];
        #endregion

        #region Properties
        public string Name
        {
            get { return StreamUtils.ToString(_name); }
        }

        public void SetName(string name)
        {
            var nameBytes = Encoding.UTF8.GetBytes(name);
            var nameLength = nameBytes.Length;
            _name = new byte[32];
            Array.Copy(nameBytes, 0, _name, 0, nameLength);
        }

        public int Attributes => StreamUtils.ToInt16(_attributes);
        public int Version => StreamUtils.ToInt16(_version);
        public DateTime? CreationDate => StreamUtils.ToDateTime(_creationDate);
        public DateTime? BackupDate => StreamUtils.ToDateTime(_backupDate);
        public DateTime? ModificationDate => StreamUtils.ToDateTime(_modificationDate);
        public int ModificationNumber => StreamUtils.ToInt32(_modificationNumber);
        public int ApplicationInfoId => StreamUtils.ToInt32(_applicationInfoId);
        public int ApplicationSortId => StreamUtils.ToInt32(_applicationSortId);
        public int Type => StreamUtils.ToInt32(_type);
        public int Creator => StreamUtils.ToInt32(_creator);
        public int UniqueIdSeed => StreamUtils.ToInt32(_uniqeIdSeed);
        public int NextRecordList => StreamUtils.ToInt32(_nextRecordList);
        public int RecordsCount => StreamUtils.ToInt32(_recordsCount);
        public long MobiHeaderSize => GetMobiHeaderSize();
        public List<PDBRecord> PDBRecords { get; } = new List<PDBRecord>();

        public int OffsetAfterMobiHeader => PDBRecords.Count > 1 ? PDBRecords[1].Offset : 0;
        #endregion


        public PDBHeader(Stream stream)
        {
            _stream = stream;
        }

        public async Task LoadPDBHeader()
        {
            Console.WriteLine($"PDB Header start: {_stream.Position}");
            await _stream.ReadBytesFromStreamAsync(_name);
            await _stream.ReadBytesFromStreamAsync(_attributes);
            await _stream.ReadBytesFromStreamAsync(_version);
            await _stream.ReadBytesFromStreamAsync(_creationDate);
            await _stream.ReadBytesFromStreamAsync(_backupDate);
            await _stream.ReadBytesFromStreamAsync(_modificationDate);
            await _stream.ReadBytesFromStreamAsync(_modificationNumber);
            await _stream.ReadBytesFromStreamAsync(_applicationInfoId);
            await _stream.ReadBytesFromStreamAsync(_applicationSortId);
            await _stream.ReadBytesFromStreamAsync(_type);
            await _stream.ReadBytesFromStreamAsync(_creator);
            await _stream.ReadBytesFromStreamAsync(_uniqeIdSeed);
            await _stream.ReadBytesFromStreamAsync(_nextRecordList);
            await _stream.ReadBytesFromStreamAsync(_recordsCount);

            //Read PDB Records
            await LoadPDBRecords();

            //Skip GapData (2 bytes)
            await _stream.ReadBytesFromStreamAsync(_gapData);

            Console.WriteLine($"PDB Header end: {_stream.Position}");

            //Console.WriteLine($"Stream position: {_stream.Position}");
            //Console.WriteLine($"PDBRecordsList.Length: {PDBRecords.Count}");
            //Console.WriteLine($"RecordsCount: {RecordsCount}");

        }

        public async Task Write(Stream stream)
        {
            await stream.WriteAsync(_name, 0, _name.Length);
            await stream.WriteAsync(_attributes, 0, _attributes.Length);
            await stream.WriteAsync(_version, 0, _version.Length);
            await stream.WriteAsync(_creationDate, 0, _creationDate.Length);
            await stream.WriteAsync(_backupDate, 0, _backupDate.Length);
            await stream.WriteAsync(_modificationDate, 0, _modificationDate.Length);
            await stream.WriteAsync(_modificationNumber, 0, _modificationNumber.Length);
            await stream.WriteAsync(_applicationInfoId, 0, _applicationInfoId.Length);
            await stream.WriteAsync(_applicationSortId, 0, _applicationSortId.Length);
            await stream.WriteAsync(_type, 0, _type.Length);
            await stream.WriteAsync(_creator, 0, _creator.Length);
            await stream.WriteAsync(_uniqeIdSeed, 0, _uniqeIdSeed.Length);
            await stream.WriteAsync(_nextRecordList, 0, _nextRecordList.Length);
            await stream.WriteAsync(_recordsCount, 0, _recordsCount.Length);

            foreach (var pdbRecord in PDBRecords)
            {
                await pdbRecord.Write(stream);
            }

            await stream.WriteAsync(_gapData, 0, _gapData.Length);
        }

        private async Task LoadPDBRecords()
        {
            for (int i = 0; i < RecordsCount; i++)
            {
                PDBRecord pdbRecord = new PDBRecord(_stream);
                await pdbRecord.LoadPDBRecord();
                PDBRecords.Add(pdbRecord);
            }
        }

        private long GetMobiHeaderSize()
        {
            return (PDBRecords.Count > 1) ? PDBRecords[1].Offset - PDBRecords[0].Offset : 0;
        }
    }
}
