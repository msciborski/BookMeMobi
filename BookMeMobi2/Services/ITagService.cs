using System.Collections.Generic;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;

namespace BookMeMobi2.Services
{
    public interface ITagService
    {
         IEnumerable<Tag> GetBookTags(int bookId);
         Task AddTagsToBookAsync(int bookid, IEnumerable<string> tags);
         IEnumerable<Tag> GetTags(TagResourceParameters parameters);
         Task DeleteTagFromBook(int bookId, int tagId);
    }
}