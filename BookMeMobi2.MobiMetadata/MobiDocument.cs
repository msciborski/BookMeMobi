using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.MobiMetadata.Headers;

namespace BookMeMobi2.MobiMetadata
{

    public class MobiDocument
    {
        private Stream _stream;

        public string Author
        {
            get => GetAuthor();
            set => SetAuthor(value);
        }

        public string Title
        {
            get => GetTitle();
            set => SetTitle(value);
        }

        public DateTime? PublishingDate
        {
            get => GetPublishingDate();
            set => SetPublishingDate(value);
        }

        public string ISBN
        {
            get => GetISBN();
            set => SetISBN(value);
        }

        public MobiDocument(Stream stream)
        {
            _stream = stream;
        }

        public PDBHeader PDBHeader { get; set; }
        public MOBIHeader MOBIHeader { get; set; }
        public CoverExtractor CoverExtractor { get; set; }


        public async Task Write(Stream stream)
        {
            await PDBHeader.Write(stream);
            await MOBIHeader.Write(stream);

            var readOffset = PDBHeader.OffsetAfterMobiHeader;
            var bytesRead = 0;
            var buffer = new byte[4096];
            _stream.Seek(readOffset, SeekOrigin.Begin);

            while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await stream.WriteAsync(buffer, 0, bytesRead);
            }
        }
        private string GetAuthor()
        {
            if (MOBIHeader == null)
            {
                return "";
            }

            return MOBIHeader.EXTHHeader.GetEXTHRecordValue(100);
        }
        private void SetAuthor(string author)
        {
            var authorBytes = Encoding.UTF8.GetBytes(author);
            MOBIHeader.EXTHHeader.ModifyExthRecord(100, authorBytes);
        }

        private string GetTitle()
        {
            if (MOBIHeader == null)
                return PDBHeader.Name;
            return MOBIHeader.FullName;
        }

        private void SetTitle(string title)
        {
            PDBHeader.SetName(title);
        }

        private DateTime? GetPublishingDate()
        {
            if (MOBIHeader == null)
            {
                return null;
            }

            var publishingDate = MOBIHeader.EXTHHeader.GetEXTHRecordValue(106);
            Console.WriteLine(publishingDate);
            return Convert.ToDateTime(publishingDate);
        }

        private void SetPublishingDate(DateTime? publishingDate)
        {
            var date = publishingDate.HasValue ? publishingDate.Value.ToShortDateString() : "";
            var dateBytes = Encoding.UTF8.GetBytes(date);
            MOBIHeader.EXTHHeader.ModifyExthRecord(106, dateBytes);
        }

        private string GetISBN()
        {
            if (MOBIHeader == null)
                return "";
            return MOBIHeader.EXTHHeader.GetEXTHRecordValue(104);
        }

        private void SetISBN(string isbn)
        {
            var isbnBytes = Encoding.UTF8.GetBytes(isbn);
            MOBIHeader.EXTHHeader.ModifyExthRecord(104, isbnBytes);
        }

    }

