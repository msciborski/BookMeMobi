using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BookMeMobi2.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _baseBookPath = "/books/";
        private readonly GoogleCredential _credential;
        private readonly GoogleCloudStorageSettings _googleCloudStorageSettings;
        private readonly string _credentialsJson;

        public StorageService(IOptions<GoogleCloudStorageSettings> options)
        {
            _googleCloudStorageSettings = options.Value;
            _credentialsJson = JsonConvert.SerializeObject(_googleCloudStorageSettings);
            _credential = GoogleCredential.FromJson(_credentialsJson);
        }
        public string UploadBook(Stream file, User user, string bookName)
        {
            var bookPath = $"{_baseBookPath}{user.Id}/{bookName}";

            using(var storage = StorageClient.Create(_credential))
            {
                var uploadedObject =
                    storage.UploadObject(_googleCloudStorageSettings.BucketName, bookPath, null, file);
            }
            return bookPath;
        }
    }
}
