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

        public async Task AddTagsToBook(int bookId, IEnumerable<string> tags)
        {
            var book = await _context.Books
              .Include(b => b.BookTags).ThenInclude(bt => bt.Book)
              .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
              .FirstOrDefaultAsync(b => b.Id == bookId);

            //Create new List if list of tags is empty
            //(then list of tags == null)
            if (book.BookTags == null)
            {
                book.BookTags = new List<BookTag>();
            }

            foreach (var tag in tags)
            {
                if (!TagExist(book, tag))
                {
                    book.BookTags.Add(new BookTag
                    {
                        Book = book,
                        Tag = new Tag
                        {
                            TagName = tag
                        }
                    });
                }
            }

            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        private bool TagExist(Book book, string tag)
        {
            var tagExist = (book.BookTags
                      .FirstOrDefault(bt => bt.Tag.TagName == tag) != null) ?
                        true : false;
            return tagExist;
        }
    }
}