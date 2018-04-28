using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Models.Book
{
    public class MobiMetadaDto
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public DateTime? PublishingDate { get; set; }
        public Stream CoverStream { get; set; }
    }
}
