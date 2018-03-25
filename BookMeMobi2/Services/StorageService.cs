using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.MobiMetadata;
using BookMeMobi2.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BookMeMobi2.Services
{
    public class StorageService : IStorageService
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        private readonly string _baseBookPath = "/books/";
        private readonly GoogleCredential _credential;
        private readonly GoogleCloudStorageSettings _googleCloudStorageSettings;

        public StorageService(IOptions<GoogleCloudStorageSettings> options, IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
            _googleCloudStorageSettings = options.Value;

            var credentialsJson = JsonConvert.SerializeObject(_googleCloudStorageSettings);
            _credential = GoogleCredential.FromJson(credentialsJson);
        }

        public async Task<BookDto> UploadBook(IFormFile file, User user)
        {
            using (var stream = file.OpenReadStream())
            {
                try
                {
                    var bookDto = await GetMobiMetadata(stream);
                    var storagePathToBook = await UploadBook(stream, user, file.FileName);
                    bookDto.UploadDate = DateTime.Now;
                    bookDto.Size = Math.Round(ConvertBytesToMegabytes(file.Length),3);

                    await AddFilesToDb(bookDto, user.Id, storagePathToBook);

                    return bookDto;
                }
                catch (Exception e)
                {
                    //log exception return null
                    throw;
                }
            }
        }
        public async Task<string> UploadBook(Stream file, User user, string bookName)
        {
            var bookPath = $"{_baseBookPath}{user.Id}/{bookName}";

            using(var storage = await StorageClient.CreateAsync(_credential))
            {
                var uploadedObject =
                     storage.UploadObject(_googleCloudStorageSettings.BucketName, bookPath, null, file);
            }
            return bookPath;
        }

        private async Task AddFilesToDb(BookDto bookDto, string userId, string storagePath)
        {
            var book = _mapper.Map<BookDto, Book>(bookDto);
            book.StoragePath = storagePath;

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            bookDto.Id = book.Id;
        }

        private async Task<BookDto> GetMobiMetadata(Stream stream)
        {
            BookDto fileDto = new BookDto();
            var mobiDocument = await MobiService.LoadDocument(stream);
            fileDto.Author = mobiDocument.Author;
            fileDto.Title = mobiDocument.Title;
            fileDto.PublishingDate = mobiDocument.PublishingDate;
            fileDto.FullName = mobiDocument.MobiHeader.FullName;
            return fileDto;
        }

        private double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}
