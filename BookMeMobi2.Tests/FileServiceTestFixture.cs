using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Mappings;
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
        public FileServiceTestFixture()
        {
            var options = SqliteInMemory.CreateOptions<ApplicationDbContext>();
            Context = new ApplicationDbContext(options);
            Context.Database.EnsureCreated();

            var loggerMock = new Mock<ILogger<BookService>>();
            Logger = loggerMock.Object;

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            Mapper = config.CreateMapper();

            GoogleCloudOptions = Microsoft.Extensions.Options.Options.Create(new GoogleCloudStorageSettings());

            FileService = new BookService(GoogleCloudOptions, Mapper, Context, Logger);

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
