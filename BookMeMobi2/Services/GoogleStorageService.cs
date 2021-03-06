﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace BookMeMobi2.Services
{
    public class GoogleStorageService : IStorageService
    {
        private readonly string _baseBookPath = "/books/";
        private readonly GoogleCredential _credential;
        private readonly GoogleCloudStorageSettings _googleCloudStorageSettings;

        public GoogleStorageService(IOptions<GoogleCloudStorageSettings> options)
        {
            _googleCloudStorageSettings = options.Value;
            _credential = GoogleCredential.GetApplicationDefault();
        }

        public async Task UploadBookAsync(Stream file, string userId, int bookId, string bookFileName)
        {
            var bookPath = $"{_baseBookPath}{userId}/{bookId}/{bookFileName}";

            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                var uploadedObject =
                    storage.UploadObject(_googleCloudStorageSettings.BucketName, bookPath, null, file);
            }
        }
        public async Task DeleteBookAsync(string userId, int bookId, string bookFileName)
        {
            var bookPath = $"{_baseBookPath}{userId}/{bookId}/{bookFileName}";
            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                await storage.DeleteObjectAsync(_googleCloudStorageSettings.BucketName, bookPath);
            }
        }

        public async Task UploadCoverAsync(Stream cover, string userId, int bookId, string bookFileName)
        {
            var coverName = $"cover{bookId}.jpg";
            var coverPath = $"{_baseBookPath}{userId}/{bookId}/{coverName}";

            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                var uploadedObject =
                    storage.UploadObject(_googleCloudStorageSettings.BucketName, coverPath, null, cover);
            }
        }

        public async Task DeleteCoverAsync(string userId, int bookId, string bookFileName)
        {
            var coverName = $"cover{bookId}.jpg";
            var coverPath = $"{_baseBookPath}{userId}/{bookId}/{coverName}";

            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                await storage.DeleteObjectAsync(_googleCloudStorageSettings.BucketName, coverPath);
            }
        }
        public async Task<Stream> DownloadBookAsync(string userId, int bookId, string bookFileName)
        {
            var bookPath = $"{_baseBookPath}{userId}/{bookId}/{bookFileName}";
            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                var stream = new MemoryStream();
                await storage.DownloadObjectAsync(_googleCloudStorageSettings.BucketName, bookPath, stream);

                return stream;
            }
        }



        public string GetDownloadUrl(string userId, int bookId, string bookFileName)
        {
            var bookPath = $"{_baseBookPath}{userId}/{bookId}/{bookFileName}";
            var initializer = new ServiceAccountCredential.Initializer(_googleCloudStorageSettings.Id);
            UrlSigner urlSgSigner = UrlSigner.FromServiceAccountCredential(
                new ServiceAccountCredential(initializer.FromPrivateKey(_googleCloudStorageSettings.PrivateKey)));
            string url = urlSgSigner.Sign(_googleCloudStorageSettings.BucketName, bookPath, TimeSpan.FromMinutes(10),
                HttpMethod.Get);
            return url;
        }

        public string GetCoverUrl(string userId, int bookId, string coverName)
        {
            var coverPath = $"{_baseBookPath}{userId}/{bookId}/{coverName}";
            var initializer = new ServiceAccountCredential.Initializer(_googleCloudStorageSettings.Id);
            UrlSigner urlSgSigner = UrlSigner.FromServiceAccountCredential(
                new ServiceAccountCredential(initializer.FromPrivateKey(_googleCloudStorageSettings.PrivateKey)));
            string url = urlSgSigner.Sign(_googleCloudStorageSettings.BucketName, coverPath, TimeSpan.FromDays(5));

            return url;
        }
    }
}
