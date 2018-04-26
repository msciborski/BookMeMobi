using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<string> UploadBookAsync(Stream file, User user, string bookName)
        {
            var bookPath = $"{_baseBookPath}{user.Id}/{bookName}";

            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                var uploadedObject =
                    storage.UploadObject(_googleCloudStorageSettings.BucketName, bookPath, null, file);
            }
            return bookPath;
        }
        public async Task<Stream> DownloadBookAsync(string storagePath)
        {
            using (var storage = await StorageClient.CreateAsync(_credential))
            {
                var stream = new MemoryStream();
                await storage.DownloadObjectAsync(_googleCloudStorageSettings.BucketName, storagePath, stream);

                return stream;
            }
        }
    }
}
