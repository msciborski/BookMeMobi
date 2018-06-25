using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Controllers;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Models;
using BookMeMobi2.Models.Book;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class FileControllerTest : IClassFixture<FileControllerTestFixture<Startup>>
    {
        private IMapper _mapper;
        private ILogger<BookController> _logger;
        private FileControllerTestFixture<Startup> _fixture;
        public FileControllerTest(FileControllerTestFixture<Startup> fixture)
        {
            _mapper = (IMapper) fixture.Server.Host.Services.GetService(typeof(IMapper));
            _fixture = fixture;
            var loggerMock = new Mock<ILogger<BookController>>();
            _logger = loggerMock.Object;
        }

        #region GetBooks (Controller)

        [Fact]
        public async Task GetBooksSuccess()
        {
            //Arrange
            var books = new List<Book>
            {
                new Book()
                {
                    Id = 1,
                    Title = "Test Title"
                },
                new Book()
                {
                    Id = 2,
                    Title = "Test Title 2"
                }
            };

            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.GetBooksForUserAsync(It.IsAny<string>(), It.IsAny<BooksResourceParameters>()))
                .Returns(books.AsEnumerable());
            var storageMock = new Mock<IStorageService>();
            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, storageMock.Object);

            //Act
            IActionResult actionResult = fileController.GetBooks("ID1", new BooksResourceParameters());

            //Assert
            actionResult.ShouldNotBeNull();

            OkObjectResult result = actionResult as OkObjectResult;

            result.StatusCode.ShouldEqual(200);

            PagedList<BookDto> modelResult = result.Value as PagedList<BookDto>;

            modelResult.Items.Count.ShouldEqual(2);
        }

        [Fact]
        public async Task GetBooksInvalidUserId()
        {
            //Arrange
            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.GetBooksForUserAsync(It.IsAny<string>(), It.IsAny<BooksResourceParameters>()))
                .Throws(new UserNoFoundException("Test"));
            var storageMock = new Mock<IStorageService>();

            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, storageMock.Object);
            
            //Act&Assert
            await Assert.ThrowsAsync<UserNoFoundException>(async () => fileController.GetBooks("ID11111", new BooksResourceParameters()));
        }

        #endregion

        #region GetBook (Controller)
        
        [Fact]
        public async Task GetBookSuccess()
        {
            //Arrange
            Cover cover = new Cover();
            cover.Id = 1;
            cover.CoverName = "asdasd";
            var book = new Book {Id = 1, Title = "Testowy Title", Cover = cover, CoverId = 1};

            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.GetBookForUserAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(book));
            var storageMock = new Mock<IStorageService>();
            storageMock.Setup(m => m.GetCoverUrl(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns("url");

            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, null);

            //Act
            IActionResult actionResult = await fileController.GetBook("ID1", 1);

            //Assert
            actionResult.ShouldNotBeNull();

            //OkObjectResult result = actionResult as OkObjectResult;
            //result.ShouldNotBeNull();
            //result.StatusCode.ShouldEqual(200);

            //BookDto resultValue = result.Value as BookDto;
            //resultValue.ShouldNotBeNull();

            //resultValue.Id.ShouldEqual(1);

        }

        [Fact]
        public async Task GetBookInvalidUserId()
        {
            //Arrange
            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.GetBookForUserAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new UserNoFoundException());
            var storageMock = new Mock<IStorageService>();
            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, storageMock.Object);

            //Act&Assert
            await Assert.ThrowsAsync<UserNoFoundException>(async () => await fileController.GetBook("ID111", 1));
        }

        [Fact]
        public async Task GetBookInvalidBookId()
        {
            //Arrange
            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.GetBookForUserAsync(It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new BookNoFoundException());
            var storageMock = new Mock<IStorageService>();
            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, storageMock.Object);

            //Act&Assert
            await Assert.ThrowsAsync<BookNoFoundException>(async () => await fileController.GetBook("ID1", 1231231));
        }


        #endregion

        #region DeleteBook (Controller)

        [Fact]
        public async Task DeleteBookSuccess()
        {
            //Arrange
            var book = new Book {Id = 1, Title = "Testowy Title"};

            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.DeleteBookAsync(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(book));
            var storageMock = new Mock<IStorageService>();
            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, storageMock.Object);

            //Act
            IActionResult actionResult = await fileController.DeleteBook("ID1", 1);

            //Assert
            actionResult.ShouldNotBeNull();

            OkObjectResult result = actionResult as OkObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(200);

            BookDeleteDto resultValue = result.Value as BookDeleteDto;
            ;
            resultValue.ShouldNotBeNull();
            resultValue.Id.ShouldEqual(1);
        }

        #endregion

        #region UploadBook (Controller)

        [Fact]
        public async Task UploadBookSuccess()
        {
            ////Arrange
            var formFilles =
                new List<IFormFile>() { new FormFile(new MemoryStream(new byte[19]), 11, 12, "Test", "test.mobi") };
            var formCollectionMock = new Mock<IFormCollection>();
            formCollectionMock.Setup(m => m.Files.GetEnumerator()).Returns(formFilles.GetEnumerator());

            var fileServcieMock = new Mock<IBookService>();
            fileServcieMock.Setup(m => m.UploadBookAsync(It.IsAny<IFormFile>(), It.IsAny<string>())).ReturnsAsync(new Book(){Id=1});

            var storageMock = new Mock<IStorageService>();
            storageMock.Setup(m =>
                m.UploadBookAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            storageMock.Setup(m => m.GetCoverUrl(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns("asd");
            var bookController = new BookController(_mapper, fileServcieMock.Object, _logger, storageMock.Object);

            //Act
            IActionResult actionResult = await bookController.UploadMobiFile(formCollectionMock.Object, "Id1");

            actionResult.ShouldNotBeNull();
            ////Act
            //IActionResult actionResult = await fileController.UploadMobiFile(formCollectionMock.Object, "Id1");

            ////Assert
            //actionResult.ShouldNotBeNull();

            //OkObjectResult result = actionResult as OkObjectResult;
            //result.ShouldNotBeNull();
            //result.StatusCode.ShouldEqual(200);

            //List<BookDto> resultValue = result.Value as List<BookDto>;
            //resultValue.Count.ShouldEqual(1);
        }

        [Fact]
        public async Task UploadBookInternalError()
        {
            //Arrange
            var formFilles =
                new List<IFormFile>() { new FormFile(new MemoryStream(new byte[19]), 11, 12, "Test", "test.mobi") };
            var formCollectionMock = new Mock<IFormCollection>();
            formCollectionMock.Setup(m => m.Files.GetEnumerator()).Returns(formFilles.GetEnumerator());

            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.UploadBookAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .Throws(new Exception());
            var storageMock = new Mock<IStorageService>();
            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, storageMock.Object);

            //Act&Assert
            await Assert.ThrowsAsync<Exception>(async () => await fileController.UploadMobiFile(formCollectionMock.Object, "ID1"));
        }

        #endregion

        #region DownloadBook (Controller)

        [Fact]
        public async Task DownloadBookSuccess()
        {
            //Arrange
            var mockStream = new Mock<Stream>();

            var fileServiceMock = new Mock<IBookService>();
            fileServiceMock.Setup(m => m.GetBookForUserAsync(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new Book() {Id = 1}));
            fileServiceMock.Setup(m => m.DownloadBookAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(mockStream.Object));
            var storageMock = new Mock<IStorageService>();
    
            var fileController = new BookController(_mapper, fileServiceMock.Object, _logger, storageMock.Object);

            //Act

            IActionResult actionResult = await fileController.DownloadBook("ID1", 1);
            actionResult.ShouldNotBeNull();

            OkObjectResult result = actionResult as OkObjectResult;
            result.ShouldNotBeNull();
        }
        #endregion
    }
}



