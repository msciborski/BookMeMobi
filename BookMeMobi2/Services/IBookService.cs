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
    public interface IBookService
    {
        Task<BookDto> UploadBookAsync(IFormFile file, string userId);
        Task<Stream> DownloadBookAsync(Book book);
        Task<Book> GetBookForUserAsync(string userId, int bookId);
        Task<PagedList<BookDto>> GetBooksForUserAsync(string userId, BooksResourceParameters parameters);
        Task<Book> DeleteBookAsync(string userId, int bookId);
        Task SendBook(string userId, int bookId);
    }
}
