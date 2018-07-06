using System.Collections.Generic;
using BookMeMobi2.Entities;

namespace BookMeMobi2.Services
{
    public interface ITagService
    {
         IEnumerable<Tag> GetBookTags(int bookId);
    }
}