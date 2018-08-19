using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Converters;
using Newtonsoft.Json;

namespace BookMeMobi2.Models.Book
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }
        public string FileName { get; set; }
        public double Size { get; set; }
        public string Format { get; set; }
        public string CoverUrl { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool IsSentToKindle { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? PublishingDate { get; set; }
        public DateTime UploadDate { get; set; }
        public IEnumerable<TagDto> Tags { get; set; }
    }
}
