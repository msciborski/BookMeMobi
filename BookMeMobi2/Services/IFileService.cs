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
        Task<BookDto> UploadBookAsync(IFormFile file, User user);
        Task<Stream> DownloadBookAsync(Book book);
        Task<Book> GetBookForUserAsync(string userId, int bookId);
        Task<PagedList<BookDto>> GetBooksForUserAsync(string userId, int pageSize, int pageNumber);
        Task<Book> DeleteBookAsync(string userId, int bookId);
    }
}
