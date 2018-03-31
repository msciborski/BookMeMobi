using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Mappings;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class FileServiceUnitTest
    {
        #region GetBookForUserAsync
        [Fact]
        public async Task ReturnBookForUserIdAndBookId()
        {
            var options = SqliteInMemory.CreateOptions<ApplicationDbContext>();
            using (var context = new ApplicationDbContext(options))
            {
                //Arrange
                context.Database.EnsureCreated();
                await context.SeedDatabaseWithUsersAndBooks();

                var mock = new Mock<ILogger<FileService>>();
                ILogger<FileService> logger = mock.Object;

                var mapperMock = new Mock<IMapper>();
                var mapper = mapperMock.Object;

                IOptions<GoogleCloudStorageSettings> googleOptions =
                    Microsoft.Extensions.Options.Options.Create(new GoogleCloudStorageSettings());

                IFileService fileService = new FileService(googleOptions, mapper, context, logger);

                //Action
                var bookDto = await fileService.GetBookForUserAsync("ID1", 1);

                //Assert
                bookDto.Id.ShouldEqual(1);
                bookDto.DeleteDate.ShouldBeNull();
                bookDto.UserId.ShouldEqual("ID1");
            }
        }

        [Fact]
        public async Task ThrowBookNoFoundExceptionForInvalidBookId()
        {
            var options = SqliteInMemory.CreateOptions<ApplicationDbContext>();
            using (var context = new ApplicationDbContext(options))
            {
                //Arrange
                context.Database.EnsureCreated();
                await context.SeedDatabaseWithUsersAndBooks();

                var mock = new Mock<ILogger<FileService>>();
                ILogger<FileService> logger = mock.Object;

                var mapperMock = new Mock<IMapper>();
                var mapper = mapperMock.Object;

                IOptions<GoogleCloudStorageSettings> googleOptions =
                    Microsoft.Extensions.Options.Options.Create(new GoogleCloudStorageSettings());

                IFileService fileService = new FileService(googleOptions, mapper, context, logger);

                //Action
                await Assert.ThrowsAsync<BookNoFoundException>(async () =>
                    await fileService.GetBookForUserAsync("ID1", 11111));

            }
        }

        [Fact]
        public async Task ThrowUserNoFoundExceptionForInvalidUserId()
        {
            var options = SqliteInMemory.CreateOptions<ApplicationDbContext>();
            using (var context = new ApplicationDbContext(options))
            {
                //Arrange
                context.Database.EnsureCreated();
                await context.SeedDatabaseWithUsersAndBooks();

                var mock = new Mock<ILogger<FileService>>();
                ILogger<FileService> logger = mock.Object;

                var mapperMock = new Mock<IMapper>();
                var mapper = mapperMock.Object;

                IOptions<GoogleCloudStorageSettings> googleOptions =
                    Microsoft.Extensions.Options.Options.Create(new GoogleCloudStorageSettings());

                IFileService fileService = new FileService(googleOptions, mapper, context, logger);

                //Action
                await Assert.ThrowsAsync<UserNoFoundException>(async () =>
                    await fileService.GetBookForUserAsync("ID1000", 1));

            }
        }


        #endregion

        #region GetBooksForUserAsync
        [Fact]
        public async Task ReturnBooksForUserId()
        {
            var options = SqliteInMemory.CreateOptions<ApplicationDbContext>();
            using (var context = new ApplicationDbContext(options))
            {
                //Arrange
                context.Database.EnsureCreated();
                await context.SeedDatabaseWithUsersAndBooks();

                var mock = new Mock<ILogger<FileService>>();
                ILogger<FileService> logger = mock.Object;
                
                var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
                var mapper = config.CreateMapper();

                IOptions<GoogleCloudStorageSettings> googleOptions =
                    Microsoft.Extensions.Options.Options.Create(new GoogleCloudStorageSettings());

                IFileService fileService = new FileService(googleOptions, mapper, context, logger);

                //Action

                await Assert.ThrowsAsync<UserNoFoundException>(async () =>
                    await fileService.GetBooksForUserAsync("ID101", 10, 1));

            }
        }

        [Fact]
        public async Task GetBooksForUserIdInvalidUserId()
        {
            var options = SqliteInMemory.CreateOptions<ApplicationDbContext>();
            using (var context = new ApplicationDbContext(options))
            {
                //Arrange
                context.Database.EnsureCreated();
                await context.SeedDatabaseWithUsersAndBooks();

                var mock = new Mock<ILogger<FileService>>();
                ILogger<FileService> logger = mock.Object;

                var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
                var mapper = config.CreateMapper();

                IOptions<GoogleCloudStorageSettings> googleOptions =
                    Microsoft.Extensions.Options.Options.Create(new GoogleCloudStorageSettings());

                IFileService fileService = new FileService(googleOptions, mapper, context, logger);

                //Action

                var result = await fileService.GetBooksForUserAsync("ID1", 10, 1);

                result.Items.Count.ShouldEqual(2);
                result.HasNextPage.ShouldEqual(false);
                result.PageSize.ShouldEqual(10);

            }
        }

        #endregion

        #region DeleteBook(soft)



        #endregion
    }
}

