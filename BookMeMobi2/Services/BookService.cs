using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Extensions;
using BookMeMobi2.MobiMetadata;
using BookMeMobi2.Models;
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

        public async Task<Book> GetBookForUserAsync(string userId, int bookId, bool withCover)
        {
            Book book = withCover ? await _context.Books.Include(b => b.Cover).FirstOrDefaultAsync(b => b.Id == bookId) : 
                await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
            return book;
        }

        public async Task<PagedList<BookDto>> GetBooksForUserAsync(string userId, BooksResourceParameters parameters)
        {
            var userBooks = parameters.WithCover
                ? _context.Books.Include(b => b.Cover).Where(b => b.UserId == userId)
                : _context.Books.Where(b => b.UserId == userId);

            //Filter method
            var books = userBooks.FilterBooks(parameters).SearchBook(parameters.SearchQuery).AsQueryable()
                .ApplySort(parameters.OrderBy, _propertyMappingService.GetPropertyMapping<BookDto, Book>());

            var booksDto = _mapper.Map<IEnumerable<Book>, IEnumerable<BookDto>>(books);
            return new PagedList<BookDto>(booksDto.AsQueryable(), parameters.PageNumber, parameters.PageSize);
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

        public async Task SendBook(string userId, int bookId)
        {
            Book book = await GetBookForUserAsync(userId, bookId, false);
            User user = await _context.Users.FindAsync(userId);
            Stream stream = await DownloadBookAsync(book);
            Attachment attachment = new Attachment(stream, book.FileName);
            await _mailService.SendMailAsync(user.KindleEmail, book.FileName, attachment);

            book.IsSentToKindle = true;
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }
        public async Task<BookDto> UploadBookAsync(IFormFile file, string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            using (var stream = file.OpenReadStream())
            {
                try
                {
                    var book = await GetMobiMetadataAsync(stream);
                    var storagePathToBook = await _storageService.UploadBookAsync(stream, user, file.FileName);
                    book.UploadDate = DateTime.Now.ToUniversalTime();
                    book.Size = Math.Round(ConvertBytesToMegabytes(file.Length), 3);
                    book.Format = GetEbookFormat(file.FileName);
                    book.FileName = file.FileName;

                    await AddFilesToDbAsync(book, user.Id, storagePathToBook);

                    var bookDto = _mapper.Map<Book, BookDto>(book);
                    return bookDto;
                }
                catch (Exception e)
                {
                    _logger.LogCritical($"Exception occured. {e.Message}. Stack trace:\n{e.StackTrace}");
                    throw;
                }
            }
        }

        private async Task AddFilesToDbAsync(Book book, string userId, string storagePath)
        {
            book.UserId = userId;
            book.StoragePath = storagePath;

            if (book.Cover != null)
            {
                await _context.Covers.AddAsync(book.Cover);
                book.CoverId = book.Cover.Id;
            }

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        private async Task<Book> GetMobiMetadataAsync(Stream stream)
        {
            Book book = new Book();
            var mobiDocument = await MobiService.LoadDocument(stream);
            book.Author = mobiDocument.Author;
            book.Title = mobiDocument.Title;
            book.PublishingDate = (mobiDocument.PublishingDate.HasValue)
                ? mobiDocument.PublishingDate?.ToUniversalTime()
                : null;

            var coverStream = mobiDocument.CoverExtractor.Extract();
            if (coverStream != null)
            {
                book.Cover = new Cover() { Image = ConvertStreamToByteArray(coverStream)};
            }

            return book;
        }

        public Task<Stream> DownloadBookAsync(Book book)
        {
            return _storageService.DownloadBookAsync(book.StoragePath);
        }

        private byte[] ConvertStreamToByteArray(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private string GetEbookFormat(string fileName)
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
