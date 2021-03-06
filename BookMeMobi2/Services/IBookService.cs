﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using BookMeMobi2.Models.Book;
using Microsoft.AspNetCore.Http;

namespace BookMeMobi2.Services
{
    public interface IBookService
    {
        Task<Book> UploadBookAsync(IFormFile file, string userId);
        Task<Stream> DownloadBookAsync(string userId, int bookId, string bookFileName);
        Task<Book> GetBookForUserAsync(string userId, int bookId);
        IQueryable<Book> GetBooksForUserAsync(string userId, UserBooksResourceParameters parameters);
        Task<Book> DeleteBookAsync(string userId, int bookId);
        Task SendBook(string userId, int bookId);
        string GetDownloadUrl(string userId, int bookId, string bookFileName);
        Task UpdateBookAsync(string userId, int bookId, BookUpdateDto model);
        IQueryable<Book> GetBooks(BooksResourceParameters parameters);
        IEnumerable<Book> GetRecommendedBooks(int bookId);
    }
}
