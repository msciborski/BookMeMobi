using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Models
{
    public class BookDeleteDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string FileName { get; set; }
        public double Size { get; set; }
        public string Format { get; set; }
        public bool IsSentToKindle { get; set; }
        public DateTime? PublishingDate { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime DeleteDate { get; set; }
        public bool IsDelete { get; set; }
    }
}
