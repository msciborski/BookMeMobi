using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.MobiMetadata;
using BookMeMobi2.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BookMeMobi2.Services
{
    public class FileService : IFileService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        private readonly string _baseBookPath = "/books/";
        private readonly GoogleCredential _credential;
        private readonly GoogleCloudStorageSettings _googleCloudStorageSettings;

        public FileService(IOptions<GoogleCloudStorageSettings> options, IMapper mapper, ApplicationDbContext context, ILogger<FileService> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _googleCloudStorageSettings = options.Value;
            _credential = GoogleCredential.GetApplicationDefault();
        }

        public async Task<Book> GetBookForUserAsync(string userId, int bookId)
        {
            var user = await _context.Users.Include(u => u.Books).FirstOrDefaultAsync(u => u.Id.Equals(userId));
            if (user == null)
            {
                _logger.LogError($"User {userId} dosen't exist.");
                throw new UserNoFoundException($"User {userId} no found.");
            }

            var book = user.Books.FirstOrDefault(b => b.Id == bookId && !b.IsDeleted);
            if (book == null)
            {
                _logger.LogError($"Book {bookId} dosen't exist.");
                throw new BookNoFoundException($"Book {bookId} no found.");
            }

            return book;
        }

        public async Task<PagedList<BookDto>> GetBooksForUserAsync(string userId, int pageSize, int pageNumber)
        {
            var user = await _context.Users.Include(u => u.Books).FirstOrDefaultAsync(u => u.Id.Equals(userId));

            if (user == null)
            {
                throw new UserNoFoundException($"User {userId} no found.");
            }

            //Filter method
            var books = user.Books.Where(b => !b.IsDeleted);

            var booksDto = _mapper.Map<IEnumerable<Book>, IEnumerable<BookDto>>(books);
            return new PagedList<BookDto>(booksDto.AsQueryable(), pageNumber, pageSize);
        }

        public async Task<Book> DeleteBookAsync(string userId, int bookId)
        {
            var user = await _context.Users.Include(u => u.Books).FirstOrDefaultAsync(u => u.Id.Equals(userId));
            if (user == null)
            {
                throw new UserNoFoundException($"User {userId} no found.");
            }

            var book = user.Books.FirstOrDefault(b => b.Id == bookId && !b.IsDeleted);
            if (book == null)
            {
                throw new BookNoFoundException($"Book {bookId} no found.");
            }

            book.IsDeleted = true;
            book.DeleteDate = DateTime.Now;

            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return book;
        }

        public async Task<BookDto> UploadBookAsync(IFormFile file, User user)
        {
            using (var stream = file.OpenReadStream())
            {
                try
                {
                    var bookDto = await GetMobiMetadataAsync(stream);
                    var storagePathToBook = await UploadBookAsync(stream, user, file.FileName);
                    bookDto.UploadDate = DateTime.Now;
                    bookDto.Size = Math.Round(ConvertBytesToMegabytes(file.Length), 3);
                    bookDto.Format = GetEbookFormat(file.FileName);
                    bookDto.FileName = file.FileName;

                    await AddFilesToDbAsync(bookDto, user.Id, storagePathToBook);

                    return bookDto;
                }
                catch (Exception e)
                {
                    _logger.LogCritical($"Exception occured. {e.Message}. Stack trace:\n{e.StackTrace}");
                    throw;
                }
            }
        }
        private async Task<string> UploadBookAsync(Stream file, User user, string bookName)
        {
            var bookPath = $"{_baseBookPath}{user.Id}/{bookName}";

            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                var uploadedObject =
                     storage.UploadObject(_googleCloudStorageSettings.BucketName, bookPath, null, file);
            }
            return bookPath;
        }

        public async Task<Stream> DownloadBookAsync(Book book)
        {
            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                var stream = new MemoryStream();
                await storage.DownloadObjectAsync(_googleCloudStorageSettings.BucketName, book.StoragePath, stream);

                return stream;
            }
        }

        private async Task AddFilesToDbAsync(BookDto bookDto, string userId, string storagePath)
        {
            var book = _mapper.Map<BookDto, Book>(bookDto);
            book.UserId = userId;
            book.StoragePath = storagePath;

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            bookDto.Id = book.Id;
        }

        private async Task<BookDto> GetMobiMetadataAsync(Stream stream)
        {
            BookDto fileDto = new BookDto();
            var mobiDocument = await MobiService.LoadDocument(stream);
            fileDto.Author = mobiDocument.Author;
            fileDto.Title = mobiDocument.Title;
            fileDto.PublishingDate = mobiDocument.PublishingDate;
            return fileDto;
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
