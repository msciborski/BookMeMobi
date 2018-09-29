using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Extensions;
using BookMeMobi2.MobiMetadata;
using BookMeMobi2.Models;
using BookMeMobi2.Models.Book;
using BookMeMobi2.Models.User;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookMeMobi2.Services
{
    public class BookService : IBookService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IMailService _mailService;
        private readonly IStorageService _storageService;

        public BookService(IMapper mapper, ApplicationDbContext context, ILogger<BookService> logger, IPropertyMappingService propertyMappingService,
          IMailService mailService, IStorageService storageService)
        {
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _propertyMappingService = propertyMappingService;
            _mailService = mailService;
            _storageService = storageService;
        }
        public IQueryable<Book> GetBooks(BooksResourceParameters parameters)
        {
          return  _context.Books
                  .Include(b => b.Cover)
                  .Include(b => b.BookTags)
                  .ThenInclude(bt => bt.Tag)
                  .FilterBooks(parameters)
                  .FilterBooksByTags(parameters.Tags)
                  .ApplySort(parameters.OrderBy, _propertyMappingService.GetPropertyMapping<BookDto, Book>());
        }

        public IEnumerable<Book> GetRecommendedBooks(int bookId) {
          var selectedBookTagId = _context.BookTags.Include(bt => bt.Tag)
                  .Where(bt => bt.BookId == bookId)
                  .Select(bt => bt.TagId);

          var recommendedBooks = _context.BookTags
                  .Include(bt => bt.Tag)
                  .Include(bt => bt.Book)
                  .ThenInclude(b => b.Cover)
                  .Where(bt => bt.BookId != bookId && selectedBookTagId.Contains(bt.TagId))
                  .GroupBy(g => new { g.BookId, g.Book })
                  .OrderByDescending(g => g.Count())
                  .Select(g => g.Key.Book)
                  .Take(6);

          return recommendedBooks;
        }

        public async Task<Book> GetBookForUserAsync(string userId, int bookId)
        {
            Book book = await _context.Books
                  .Include(b => b.Cover)
                  .Include(b => b.BookTags)
                  .ThenInclude(bt => bt.Tag)
                  .FirstOrDefaultAsync(b => b.Id == bookId && b.UserId == userId);
            return book;
        }

        public IQueryable<Book> GetBooksForUserAsync(string userId, UserBooksResourceParameters parameters)
        {
            var userBooks = _context.Books
                  .Include(b => b.Cover)
                  .Include(b => b.BookTags)
                  .ThenInclude(bt => bt.Tag)
                  .Where(b => b.UserId == userId)
                  .FilterBooks(parameters)
                  .FilterBooksByTags(parameters.Tags)
                  .ApplySort(parameters.OrderBy, _propertyMappingService.GetPropertyMapping<BookDto, Book>());

            return userBooks;
        }

        public async Task<Book> DeleteBookAsync(string userId, int bookId)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
            book.IsDeleted = true;
            book.DeleteDate = DateTime.Now.ToUniversalTime();

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return book;
        }

        public async Task UpdateBookAsync(string userId, int bookId, BookUpdateDto model)
        {
            Book book = null;
            Stream bookStream = null;
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    book = await GetBookForUserAsync(userId, bookId);
                    bookStream = await DownloadBookAsync(userId, bookId, book.FileName);
                    bookStream.Position = 0;
                    var mobiDocument = await MobiService.GetMobiDocument(bookStream);

                    var editedBookStream = await UpdateMobiMetadata(mobiDocument, model);
                    editedBookStream.Position = 0;
                    await _storageService.UploadBookAsync(editedBookStream, userId, bookId, book.FileName);

                    await UpdateBookDb(book, model);
                    transaction.Commit();
                }
                catch (DbException e)
                {
                    transaction.Rollback();
                    bookStream.Position = 0;
                    var mobiDocument = await MobiService.GetMobiDocument(bookStream);
                    var editedBookStream = await UpdateMobiMetadata(mobiDocument, new BookUpdateDto { Title = book.Title, Author = book.Author });
                    await _storageService.UploadBookAsync(editedBookStream, userId, book.Id, book.FileName);
                    _logger.LogCritical(e.ToString());
                    throw;
                }
            }

        }

        public async Task<Stream> UpdateMobiMetadata(MobiDocument mobiDocument, BookUpdateDto model)
        {
            if (!String.IsNullOrEmpty(model.Author))
            {
                mobiDocument.Author = model.Author;
            }

            if (!String.IsNullOrEmpty(model.Title))
            {
                mobiDocument.Title = model.Title;
            }
            if(model.PublishingDate != null && model.PublishingDate.HasValue)
            {
              mobiDocument.PublishingDate = model.PublishingDate;
            }

            return await MobiService.SaveMobiDocument(mobiDocument);
        }

        public async Task UpdateBookDb(Book book, BookUpdateDto model)
        {
            try
            {
                if (!String.IsNullOrEmpty(model.Author))
                {
                    book.Author = model.Author;
                }

                if (!String.IsNullOrEmpty(model.Title))
                {
                    book.Title = model.Title;
                }

                if(model.PublishingDate != null && model.PublishingDate.HasValue)
                {
                  book.PublishingDate = model.PublishingDate;
                }
                book.IsDeleted = model.IsDeleted;
                book.IsPublic = model.IsPublic;
                book.HasBeenEdited = true;
                book.LastEditDate = DateTime.Now.ToUniversalTime();

                _context.Books.Update(book);
                await _context.SaveChangesAsync();


            }
            catch (Exception e)
            {
                throw new DbException(e.Message, e.InnerException);
            }

        }

        public async Task SendBook(string userId, int bookId)
        {
            Book book = await GetBookForUserAsync(userId, bookId);
            User user = await _context.Users.FindAsync(userId);
            Stream stream = await DownloadBookAsync(userId, bookId, book.FileName);
            await _mailService.SendMailAsync(user.Email, $"{book.Title}", null, stream, book.FileName);

            book.IsSentToKindle = true;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task<Book> UploadBookAsync(IFormFile file, string userId)
        {

            var user = await _context.Users.FindAsync(userId);
            Book book = null;

            using (var stream = file.OpenReadStream())
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var metadata = await GetMobiMetadataAsync(stream);

                    book = new Book
                    {
                        Author = metadata.Author,
                        //If title dosen't exist in mobi metdata, try to get it from file name
                        Title = String.IsNullOrEmpty(metadata.Title) || String.IsNullOrWhiteSpace(metadata.Title) ?
                                        GetTitleFromFileName(file.FileName, metadata.Author) : metadata.Title,
                        PublishingDate = metadata.PublishingDate,
                        UploadDate = DateTime.Now.ToUniversalTime(),
                        Format = BookService.GetEbookFormat(file.FileName),
                        Size = Math.Round(BookService.ConvertBytesToMegabytes(file.Length), 3),
                        FileName = file.FileName
                    };
                    await AddFilesToDbAsync(book, userId);
                    await _storageService.UploadBookAsync(stream, userId, book.Id, file.FileName);

                    if (metadata.CoverStream != null)
                    {
                        var coverName = $"cover{book.Title}.jpg";
                        await _storageService.UploadCoverAsync(metadata.CoverStream, userId, book.Id, coverName);
                        await AddCoverToDb(book, coverName);
                    }

                    transaction.Commit();

                    return book;
                }
                catch (Exception e)
                {
                    await RollBackChangesInStorage(book, userId);
                    transaction.Rollback();

                    _logger.LogCritical($"Exception occured. {e.Message}. Stack trace:\n{e.StackTrace}");
                    throw;
                }
            }

        }
        private async Task RollBackChangesInStorage(Book book, string userId)
        {
            if (book != null)
            {
                await _storageService.DeleteBookAsync(userId, book.Id, book.FileName);
                if (book.Cover != null)
                {
                    await _storageService.DeleteCoverAsync(userId, book.Id, book.FileName);
                }

            }
        }

        private async Task AddFilesToDbAsync(Book book, string userId)
        {
            book.UserId = userId;
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        private async Task AddCoverToDb(Book book, string coverName)
        {
            Cover cover = new Cover() { CoverName = coverName };
            await _context.Covers.AddAsync(cover);

            book.Cover = cover;
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        //Method tries to get title from file name
        private static string GetTitleFromFileName(string fileName, string author)
        {
            var title = String.Empty;
            //if file name contains author -> remove it
            if (fileName.Contains(author))
            {
                title = fileName.Replace(author, String.Empty);
            }

            //Remove extension from title
            if (title.Contains(".mobi"))
            {
                title = title.Replace(".mobi", String.Empty);
            }
            else if (title.Contains(".epub"))
            {
                title = title.Replace(".epub", String.Empty);
            }

            //Remove special characters from start and end of file name
            title = Regex.Replace(title, "(^[^\\w0-9_.]+|[^\\w0-9_.]+$)", String.Empty);
            //Remove starting and tralining spaces
            title = title.Trim();
            return title;
        }

        private static async Task<MobiMetadaDto> GetMobiMetadataAsync(Stream stream)
        {
            MobiMetadaDto mobiMetada = new MobiMetadaDto();
            var mobiDocument = await MobiService.GetMobiDocument(stream);
            mobiMetada.Author = mobiDocument.Author;
            mobiMetada.Title = mobiDocument.Title;
            mobiMetada.PublishingDate = (mobiDocument.PublishingDate.HasValue) ?
              mobiDocument.PublishingDate?.ToUniversalTime() :
              null;
            mobiMetada.Isbn = mobiDocument.ISBN;

            var coverStream = mobiDocument.CoverExtractor.Extract();
            if (coverStream != null)
            {
                mobiMetada.CoverStream = coverStream;
            }

            return mobiMetada;
        }

        public Task<Stream> DownloadBookAsync(string userId, int bookId, string bookFileName)
        {
            return _storageService.DownloadBookAsync(userId, bookId, bookFileName);
        }

        public string GetDownloadUrl(string userId, int bookId, string bookFileName)
        {
            return _storageService.GetDownloadUrl(userId, bookId, bookFileName);
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private static string GetEbookFormat(string fileName)
        {
            if (fileName.Contains(".mobi"))
            {
                return "mobi";
            }

            if (fileName.Contains(".azw3"))
            {
                return "azw3";
            }
            throw new NotImplementedException("Format is not avaiable.");
        }

    }
}