﻿using System;
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

        public async Task<Book> GetBookForUserAsync(string userId, int bookId)
        {
            Book book = await _context.Books.Include(b => b.Cover).FirstOrDefaultAsync(b => b.Id == bookId);
            return book;
        }

        public async Task<IEnumerable<Book>> GetBooksForUserAsync(string userId, BooksResourceParameters parameters)
        {
            var userBooks = _context.Books.Include(b => b.Cover).Where(b => b.UserId == userId);

            //Filter method
            var books = userBooks.FilterBooks(parameters).SearchBook(parameters.SearchQuery).AsQueryable()
              .ApplySort(parameters.OrderBy, _propertyMappingService.GetPropertyMapping<BookDto, Book>());
            return books;
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

                }
                catch (DbException e)
                {
                    transaction.Rollback();
                    bookStream.Position = 0;
                    var mobiDocument = await MobiService.GetMobiDocument(bookStream);
                    var editedBookStream = await UpdateMobiMetadata(mobiDocument, new BookUpdateDto { Title = book.Title, Author = book.Author });
                    await _storageService.UploadBookAsync(editedBookStream, userId, book.Id, book.FileName);
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
            Cover cover = null;
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
                        Title = metadata.Title,
                        PublishingDate = metadata.PublishingDate,
                        UploadDate = DateTime.Now.ToUniversalTime(),
                        Format = GetEbookFormat(file.FileName),
                        Size = Math.Round(ConvertBytesToMegabytes(file.Length), 3),
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

        private async Task<MobiMetadaDto> GetMobiMetadataAsync(Stream stream)
        {
            MobiMetadaDto mobiMetada = new MobiMetadaDto();
            var mobiDocument = await MobiService.GetMobiDocument(stream);
            mobiMetada.Author = mobiDocument.Author;
            mobiMetada.Title = mobiDocument.Title;
            mobiMetada.PublishingDate = (mobiDocument.PublishingDate.HasValue) ?
              mobiDocument.PublishingDate?.ToUniversalTime() :
              null;

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