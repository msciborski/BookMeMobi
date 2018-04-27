using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Mappings;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TestSupport.EfHelpers;

namespace BookMeMobi2.Tests
{
    public class FileServiceTestFixture : IDisposable
    {
        public ApplicationDbContext Context { get; private set; }
        public ILogger<BookService> Logger { get; private set; }
        public IMapper Mapper { get; private set; }
        public IOptions<GoogleCloudStorageSettings> GoogleCloudOptions { get; private set; }
        public BookService FileService { get; private set; }
        private Dictionary<string, PropertyMappingValue> _booksPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new PropertyMappingValue(new List<string>(){"Id"}) },
            { "Title", new PropertyMappingValue(new List<string>(){"Title"}) },
            { "Author", new PropertyMappingValue(new List<string>(){"Author"}) },
            { "FileName", new PropertyMappingValue(new List<string>(){"FileName"}) },
            { "PublishingDate", new PropertyMappingValue(new List<string>(){"PublishingDate"}) }
        };
        public FileServiceTestFixture()
        {

            var propertyMappingServiceMock = new Mock<IPropertyMappingService>();
            propertyMappingServiceMock.Setup(m => m.GetPropertyMapping<BookDto, Book>())
                .Returns(_booksPropertyMapping);

            var options = SqliteInMemory.CreateOptions<ApplicationDbContext>();
            Context = new ApplicationDbContext(options);
            Context.Database.EnsureCreated();

            var loggerMock = new Mock<ILogger<BookService>>();
            Logger = loggerMock.Object;

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            Mapper = config.CreateMapper();

            GoogleCloudOptions = Microsoft.Extensions.Options.Options.Create(new GoogleCloudStorageSettings());

            var mailServiceMock = new Mock<IMailService>();
            var storageServiceMock = new Mock<IStorageService>();
            storageServiceMock.Setup(m => m.GetDownloadUrl(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns("");
            storageServiceMock.Setup(m => m.UploadBookAsync(It.IsAny<Stream>(), It.IsAny<string>(),It.IsAny<int>(), It.IsAny<string>()));
            storageServiceMock.Setup(m => m.DownloadBookAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            //FileService = new BookService(GoogleCloudOptions, Mapper, Context, Logger, propertyMappingServiceMock.Object, mailServiceMock.Object);
            FileService = new BookService(Mapper, Context, Logger, propertyMappingServiceMock.Object, mailServiceMock.Object, storageServiceMock.Object );
        }

        public async Task SeedDatabase()
        {
            await Context.SeedDatabaseWithUsersAndBooks();
        }
        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
