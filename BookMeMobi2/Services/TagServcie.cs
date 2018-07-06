using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using Microsoft.EntityFrameworkCore;

namespace BookMeMobi2.Services
{
    public class TagServcie : ITagService
    {
      private readonly ApplicationDbContext _context;
      public TagServcie(ApplicationDbContext context)
      {
        _context = context;
      }

      public IEnumerable<Tag> GetBookTags(int bookId)
      {
        var tags = _context.BookTags.Include(bt => bt.Tag).Where(bt => bt.BookId == bookId).Select(bt => bt.Tag);
        return tags;
      }
    }
}