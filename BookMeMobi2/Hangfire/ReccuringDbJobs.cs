using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Services;
using Microsoft.EntityFrameworkCore;

namespace BookMeMobi2.Hangfire
{
    public class ReccuringDbJobs
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storage;
        public ReccuringDbJobs(ApplicationDbContext context, IStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        public async Task DeleteSoftDeletedBooksOlderThan30DaysAsync()
        {
            var booksToHardDelete = _context.Books.Include(b => b.User).Include(b => b.Cover).Where(b => b.IsDeleted && (DateTime.Now - b.DeleteDate.Value).TotalDays >= 30);
            var coversToHardDelete = booksToHardDelete.Select(b => b.Cover);
            foreach (var book in booksToHardDelete)
            {
                await _storage.DeleteBookAsync(book.UserId, book.Id, book.FileName);
                await _storage.DeleteCoverAsync(book.UserId, book.Id, book.FileName);
            }

            _context.Covers.RemoveRange(coversToHardDelete);
            _context.Books.RemoveRange(booksToHardDelete);
            await _context.SaveChangesAsync();
        }
    }
}
