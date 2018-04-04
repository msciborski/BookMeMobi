using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Controllers;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Mappings;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class UserControllerTest
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UserControllerTest()
        {
            var loggerMock = new Mock<ILogger<UsersController>>();
            _logger = loggerMock.Object;

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();
        }

        #region SignIn (Controller)

        [Fact]
        public async Task SignInSuccess()
        {
            //Arrange
            var credentials = new Credentials {Password = "Test!123@", Username = "Test1"};
            
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.SignIn(It.IsAny<Credentials>())).ReturnsAsync(new UserLoginDto() {Id = "ID1"});

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act
            IActionResult actionResult = await usersController.SignIn(credentials);

            //Assert
            actionResult.ShouldNotBeNull();

            OkObjectResult result = actionResult as OkObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(200);

            var userLoginDto = result.Value as UserLoginDto;
            userLoginDto.ShouldNotBeNull();
            userLoginDto.Id.ShouldEqual("ID1");
        }

        [Fact]
        public async Task SignInFailureBadCredentials()
        {
            //Arrange
            var credentials = new Credentials { Password = "Test!123@", Username = "Test1" };

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.SignIn(It.IsAny<Credentials>())).Throws(new AppException());

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act
            IActionResult actionResult = await usersController.SignIn(credentials);

            //Assert
            actionResult.ShouldNotBeNull();

            NotFoundObjectResult result = actionResult as NotFoundObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(404);
        }

        #endregion

        #region Register (Controller)

        [Fact]
        public async Task RegisterSuccess()
        {
            //Arrange

            var userRegisterDto = new UserRegisterDto(){Email = "test@test.com", FirstName = "Test", LastName = "Test", Password = "Test!231@", UserName = "Test"};

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.Register(It.IsAny<UserRegisterDto>()))
                .ReturnsAsync(new UserLoginDto() {Email = "test@test.com", FirstName = "TestName"});

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act
            IActionResult actionResult = await usersController.Register(userRegisterDto);

            //Assert
            actionResult.ShouldNotBeNull();

            JsonResult result = actionResult as JsonResult;
            result.StatusCode.ShouldEqual(201);

            UserLoginDto resultValue = result.Value as UserLoginDto;
            resultValue.Email.ShouldEqual("test@test.com");
        }

        [Fact]
        public async Task RegisterFailed()
        {
            var userRegisterDto = new UserRegisterDto()
            {
                Email = "test@test.com",
                FirstName = "TestName",
                LastName = "TestLast",
                Password = "1231",
                UserName = "userfail"
            };

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.Register(userRegisterDto)).Throws(new AppException());

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act
            IActionResult actionResult = await usersController.Register(userRegisterDto);
            actionResult.ShouldNotBeNull();

            BadRequestObjectResult result = actionResult as BadRequestObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(400);
            
        }
        #endregion

        #region GetAllUsers (Controller)

        [Fact]
        public async Task GetAllUsers()
        {
            //Arrange
            var usersDto = new List<UserDto>
            {
                new UserDto
                {
                    Id = "ID1",
                    UserName = "Test1"
                },
                new UserDto
                {
                    Id = "ID2",
                    UserName = "Test2"
                }
            };
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.GetAllUsers(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PagedList<UserDto>(usersDto.AsQueryable(), 1, 10));

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act

            IActionResult actionResult = usersController.GetAllUsers();

            //Assert
            actionResult.ShouldNotBeNull();

            OkObjectResult result = actionResult as OkObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(200);

            PagedList<UserDto> resultValue = result.Value as PagedList<UserDto>;
            resultValue.Items.Count.ShouldEqual(2);
        }

        #endregion

        #region GetUser (Controller)

        [Fact]
        public async Task GetUserSuccess()
        {
            //Arrange
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.GetUser(It.IsAny<string>()))
                .ReturnsAsync(new User() {Id = "ID1", UserName = "Test1"});

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act
            IActionResult actionResult = await usersController.GetUser("ID1");

            //Assert
            actionResult.ShouldNotBeNull();

            OkObjectResult result = actionResult as OkObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(200);

            UserDto resultValue = result.Value as UserDto;
            resultValue.ShouldNotBeNull();
            resultValue.Id.ShouldEqual("ID1");
        }

        [Fact]
        public async Task GeUserInvalidUserId()
        {
            //Arrange
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.GetUser(It.IsAny<string>()))
                .Throws(new UserNoFoundException());

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act
            IActionResult actionResult = await usersController.GetUser("ID1");

            //Assert
            actionResult.ShouldNotBeNull();

            NotFoundObjectResult result = actionResult as NotFoundObjectResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(404);
        }

        [Fact]
        public async Task GeUserInternalError()
        {
            //Arrange
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(m => m.GetUser(It.IsAny<string>()))
                .Throws(new Exception());

            var usersController = new UsersController(userServiceMock.Object, _logger, _mapper);

            //Act
            IActionResult actionResult = await usersController.GetUser("ID1");

            //Assert
            actionResult.ShouldNotBeNull();

            JsonResult result = actionResult as JsonResult;
            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(500);
        }

        #endregion
    }
}

