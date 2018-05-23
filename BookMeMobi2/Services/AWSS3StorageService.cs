using System.IO;
using System.Threading.Tasks;
using BookMeMobi2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using A
using Amazon.S3;
using Amazon.S3.Model;
using BookMeMobi2.Helpers.Exceptions;
using Amazon.Runtime;
using System.Text;
using System;

namespace BookMeMobi2.Services
{
    public class AWSS3StorageService : IStorageService
    {
        private readonly string _bookPath = "book/{userId}/{bookId}/{bookFileName}";
        private readonly string _coverPath = "book/{userId}/{bookId}/cover{bookId}.jpg";
        private readonly AWSS3Settings _AWSS3Settings;
        private readonly ILogger<AWSS3StorageService> _logger;
        private IAmazonS3 storageClient;

        public AWSS3StorageService(IOptions<AWSS3Settings> settings, ILogger<AWSS3StorageService> logger)
        {
            _AWSS3Settings = settings.Value;
            _logger = logger;
            storageClient = new AmazonS3Client(_AWSS3Settings.AccessKeyId, _AWSS3Settings.SecretKey,
                        Amazon.RegionEndpoint.EUCentral1);
        }

        public Task DeleteBookAsync(string userId, int bookId, string bookFileName)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteCoverAsync(string userId, int bookId, string bookFileName)
        {
            throw new System.NotImplementedException();
        }

        public Task<Stream> DownloadBookAsync(string userId, int bookId, string bookFileName)
        {
            throw new System.NotImplementedException();
        }

        public string GetCoverUrl(string userId, int bookId, string coverName)
        {
            var coverPath = $"{_coverPath}";
            return GetPreSignedUrl(coverPath, DateTime.Now.AddHours(24));
        }

        public string GetDownloadUrl(string userId, int bookId, string bookFileName)
        {
            var bookPath  =  $"{_bookPath}";
            return GetPreSignedUrl(bookPath, DateTime.Now.AddMinutes(5));
        }
        private string GetPreSignedUrl(string path, DateTime expirationTime)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _AWSS3Settings.BucketName,
                Key = path,
                Expires = expirationTime
            };

            return storageClient.GetPreSignedURL(request);
        }

        public async Task UploadBookAsync(Stream file, string userId, int bookId, string bookFileName)
        {
            var bookPath = $"{_bookPath}";


            if (file != null || file.Length <= 0)
            {
                var request = new PutObjectRequest
                {
                    BucketName = _AWSS3Settings.BucketName,
                    Key = bookPath,
                    InputStream = file,
                    CannedACL = S3CannedACL.Private
                };

                var response = await storageClient.PutObjectAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new AppException(CreateErrorFromResponseMetada(response.ResponseMetadata),
                                500);
                }
            }
            throw new AppException("Book stream is empty.", 400);
        }

        public async Task<string> UploadCoverAsync(Stream cover, string userId, int bookId, string bookFileName)
        {
            var coverPath = $"{_coverPath}";

            if (cover != null || cover.Length <= 0)
            {
                var request = new PutObjectRequest
                {
                    BucketName = _AWSS3Settings.BucketName,
                    Key = coverPath,
                    InputStream = cover,
                    CannedACL = S3CannedACL.Private
                };

                var response = await storageClient.PutObjectAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new AppException(CreateErrorFromResponseMetada(response.ResponseMetadata),
                                500);
                }

            }
            throw new AppException("Cover stream is empty.", 400);

        }
        private string CreateErrorFromResponseMetada(ResponseMetadata responseMetadata)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var keyValue in responseMetadata.Metadata)
            {
                stringBuilder.Append($"{keyValue.Key}: {keyValue.Value}");
            }

            return stringBuilder.ToString();
        }
    }
}