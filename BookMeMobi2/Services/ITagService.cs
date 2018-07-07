using System.Collections.Generic;
using System.Threading.Tasks;
using BookMeMobi2.Entities;

namespace BookMeMobi2.Services
{
    public interface ITagService
    {
         IEnumerable<Tag> GetBookTags(int bookId);
         Task AddTagsToBook(int bookid, IEnumerable<string> tags);
    }
}