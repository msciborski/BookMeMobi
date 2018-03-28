using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using Microsoft.AspNetCore.Http;

namespace BookMeMobi2.Services
{
    public interface IFileService
    {
        Task<BookDto> UploadBook(IFormFile file, User user);
        Task<Stream> DownloadBook(Book book);
        Task<Book> GetBookForUser(string userId, int bookId);
        Task<PagedList<BookDto>> GetBooksForUser(string userId, int pageSize, int pageNumber);
    }
}
