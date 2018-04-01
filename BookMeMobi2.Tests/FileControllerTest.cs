using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Controllers;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class FileControllerTest : IClassFixture<FileControllerTestFixture>
    {
        private FileControllerTestFixture _fixture;
        private IMapper _mapper;

        public FileControllerTest(FileControllerTestFixture fixture)
        {
            _fixture = fixture;
        }

        #region GetBooks (Controller)

        [Fact]
        public async Task GetBooksSuccess()
        {
            //Arrange
            var books = new List<BookDto>
            {
                new BookDto()
                {
                    Id = 1,
                    Title = "Test Title"
                },
                new BookDto()
                {
                    Id = 2,
                    Title = "Test Title 2"
                }
            };

            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(m => m.GetBooksForUserAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new PagedList<BookDto>(books.AsQueryable(), 1, 10)));

            var fileController = new FilesController(_fixture.Mapper, fileServiceMock.Object, _fixture.Logger);

            //Act
            IActionResult actionResult = await fileController.GetBooks("ID1");

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
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(m => m.GetBooksForUserAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new UserNoFoundException("Test"));

            var fileController = new FilesController(_fixture.Mapper, fileServiceMock.Object, _fixture.Logger);
            
            //Act
            IActionResult actionResult = await fileController.GetBooks("ID101");

            //Assert
            actionResult.ShouldNotBeNull();

            NotFoundObjectResult result = actionResult as NotFoundObjectResult;
            result.ShouldNotBeNull();

            result.StatusCode.ShouldEqual(404);
        }

        #endregion

        #region GetBook (Controller)
        
        [Fact]
        public async Task GetBookSuccess()
        {
            //Arrange
            var book = new Book {Id = 1, Title = "Testowy Title"};

            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(m => m.GetBookForUserAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(book));

            var fileController = new FilesController(_fixture.Mapper, fileServiceMock.Object, _fixture.Logger);

            //Act
            IActionResult actionResult = await fileController.GetBook("ID1", 1);

            //Assert
            actionResult.ShouldNotBeNull();

            OkObjectResult result = actionResult as OkObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(200);

            BookDto resultValue = result.Value as BookDto;
            resultValue.ShouldNotBeNull();

            resultValue.Id.ShouldEqual(1);

        }

        [Fact]
        public async Task GetBookInvalidUserId()
        {
            //Arrange
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(m => m.GetBookForUserAsync(It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new UserNoFoundException());

            var fileController = new FilesController(_fixture.Mapper, fileServiceMock.Object, _fixture.Logger);

            //Act
            IActionResult actionResult = await fileController.GetBook("ID112", 1);

            //Assert
            actionResult.ShouldNotBeNull();

            NotFoundObjectResult result = actionResult as NotFoundObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(404);
        }

        [Fact]
        public async Task GetBookInvalidBookId()
        {
            //Arrange
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(m => m.GetBookForUserAsync(It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new BookNoFoundException());

            var fileController = new FilesController(_fixture.Mapper, fileServiceMock.Object, _fixture.Logger);

            //Act
            IActionResult actionResult = await fileController.GetBook("ID1", 121312312);

            //Assert
            actionResult.ShouldNotBeNull();

            NotFoundObjectResult result = actionResult as NotFoundObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(404);
        }


        #endregion
    }
}
