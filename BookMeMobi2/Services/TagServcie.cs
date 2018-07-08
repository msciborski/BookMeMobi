using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Extensions;
using BookMeMobi2.Models;
using Microsoft.EntityFrameworkCore;

namespace BookMeMobi2.Services
{
    public class TagServcie : ITagService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPropertyMappingService _propertyMappingService;
        public TagServcie(ApplicationDbContext context, IPropertyMappingService propertyMappingService)
        {
            _context = context;
            _propertyMappingService = propertyMappingService;
        }

        public IEnumerable<Tag> GetTags(TagResourceParameters parameters)
        {
            var tags = _context.Tags;

            var retrunTags = tags.SearchTag(parameters.TagName).AsQueryable().ApplySort(parameters.OrderBy, _propertyMappingService.GetPropertyMapping<TagDto, Tag>());
            return retrunTags;
        }
        public IEnumerable<Tag> GetBookTags(int bookId)
        {
            var tags = _context.BookTags
              .Include(bt => bt.Tag)
              .Where(bt => bt.BookId == bookId)
              .Select(bt => bt.Tag);

            return tags;
        }
        //TODO: Do przepisania jutro, bo to co ty tu odjebales to wstyd
        public async Task AddTagsToBookAsync(int bookId, IEnumerable<string> tags)
        {
            var book = await _context.Books
              .Include(b => b.BookTags).ThenInclude(bt => bt.Book)
              .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
              .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book != null)
            {
                foreach (var tag in tags)
                {
                    Tag tagToAdd = null;

                    if (await TagExist(tag))
                    {
                        if (!TagAddedToBook(book, tag))
                        {
                            tagToAdd = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tag);
                            tagToAdd.CountUsage++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        tagToAdd = new Tag { TagName = tag, CountUsage = 1 };
                    }

                    book.BookTags.Add(new BookTag { Book = book, Tag = tagToAdd });
                }
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new AppException("Book dosen't exist.", 404);
            }
        }
        public async Task DeleteTagFromBook(int bookId, int tagId)
        {
            var bookTag = await _context.BookTags
                                .Include(bt => bt.Tag)
                                .FirstOrDefaultAsync(bt => bt.BookId == bookId && bt.TagId == tagId);
            if (bookTag != null)
            {
                bookTag.Tag.CountUsage--;
                _context.BookTags.Remove(bookTag);

                await _context.SaveChangesAsync();
            }
            else
            {
              throw new AppException("Tag to remove dosen't exist.", 404);
            }
        }
        private async Task<bool> TagExist(string tag)
        {
            var tagExist = await _context.Tags.AllAsync(t => t.TagName != tag);
            return !tagExist;
        }
        private bool TagAddedToBook(Book book, string tag)
        {
            var tagExistInBook = book.BookTags.Any(b => b.Tag.TagName == tag);
            return tagExistInBook;
        }
    }
}