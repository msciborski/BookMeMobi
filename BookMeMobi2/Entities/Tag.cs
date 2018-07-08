using System.Collections.Generic;

namespace BookMeMobi2.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public int CountUsage { get; set; }
        public ICollection<BookTag> BookTags { get; set; }

    }
}