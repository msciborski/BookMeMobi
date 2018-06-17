using System.IO;
using System.Threading.Tasks;
using BookMeMobi2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon.S3.Model;
using BookMeMobi2.Helpers.Exceptions;
using Amazon.Runtime;
using System.Text;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace BookMeMobi2.Services
{
    public class AWSS3StorageService : IStorageService
    {
        private readonly string _bookPath = "book/{0}/{1}/{2}";
        private readonly string _coverPath = "book/{0}/{1}/{2}";
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

        public async Task DeleteBookAsync(string userId, int bookId, string bookFileName)
        {
            var bookPath = String.Format(_bookPath, userId, bookId, bookFileName);
            await DeleteFile(bookPath);
        }

        public async Task DeleteCoverAsync(string userId, int bookId, string bookFileName)
        {
            var coverPath = String.Format(_coverPath, userId, bookId, bookFileName);
            await DeleteFile(coverPath);
        }

        private async Task DeleteFile(string path)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _AWSS3Settings.BucketName,
                Key = path
            };
            await storageClient.DeleteObjectAsync(request);
        }

        public async Task<Stream> DownloadBookAsync(string userId, int bookId, string bookFileName)
        {
            var bookPath = String.Format(_bookPath, userId, bookId, bookFileName);
            var request = new GetObjectRequest
            {
                BucketName = _AWSS3Settings.BucketName,
                Key = bookPath
            };
            var response = await storageClient.GetObjectAsync(request);

            //Check response http code if download succseful
            Stream stream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(stream);

            return stream;
        }

        public string GetCoverUrl(string userId, int bookId, string coverName)
        {
            var coverPath = String.Format(_coverPath, userId, bookId, coverName);
            return GetPreSignedUrl(coverPath, DateTime.Now.AddHours(24));
        }

        public string GetDownloadUrl(string userId, int bookId, string bookFileName)
        {
            var bookPath = String.Format(_bookPath, userId, bookId, bookFileName);
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
            var bookPath = String.Format(_bookPath, userId, bookId, bookFileName);
            await UploadFile(file, bookPath);
        }

        public async Task UploadCoverAsync(Stream cover, string userId, int bookId, string bookFileName)
        {
            var coverPath = String.Format(_coverPath, userId, bookId, bookFileName);
            await UploadFile(cover, coverPath);
        }

        private async Task UploadFile(Stream file, string path)
        {
            if (file == null || file.Length <= 0)
            {

                throw new AppException("Book stream is empty.", 400);

            }
            var request = new PutObjectRequest
            {
                BucketName = _AWSS3Settings.BucketName,
                Key = path,
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