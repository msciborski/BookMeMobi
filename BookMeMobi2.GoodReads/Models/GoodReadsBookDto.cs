using System;

namespace BookMeMobi2.GoodReads.Models
{
    public class GoodReadsBookDto
    {
        public DateTime? PublishingDate { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
}