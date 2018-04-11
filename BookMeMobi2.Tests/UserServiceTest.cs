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
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class UserServiceTest
    {
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenService _tokenService;

        public UserServiceTest()
        {
            var loggerMock = new Mock<ILogger<UserService>>();
            _logger = loggerMock.Object;

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();

            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(m => m.CreateToken(It.IsAny<User>()))
                .Returns("Token based authentication is awesome.");
            _tokenService = tokenServiceMock.Object;
        }

        #region SignIn

        [Fact]
        public async Task SignInUserSuccess()
        {
            //Arrange
            var credentials = new Credentials() { Password = "SłabeHasło!123", Username = "TestUserName" };
            var user = new User() { Id = "ID1", UserName = "TestUserName" };

            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.FindByNameAsync(credentials.Username)).ReturnsAsync(user);

            var fakeSignInManager = new Mock<FakeSignInManager>();
            fakeSignInManager.Setup(
                m => m.PasswordSignInAsync(credentials.Username, credentials.Password, false, false)).ReturnsAsync(SignInResult.Success);

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object, fakeSignInManager.Object, _tokenService);

            //Act
            var result = await userService.SignIn(credentials);

            //Assert
            result.ShouldNotBeNull();
            result.Id.ShouldEqual("ID1");
        }

        [Fact]
        public async Task SingInWrongCredentials()
        {
            //Arrange
            var credentials = new Credentials() { Password = "SłabeHasło!123", Username = "TestUserName" };
            var user = new User() { Id = "ID1", UserName = "TestUserName" };

            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.FindByNameAsync(credentials.Username)).ReturnsAsync(user);

            var fakeSignInManager = new Mock<FakeSignInManager>();
            fakeSignInManager.Setup(
                m => m.PasswordSignInAsync(credentials.Username, credentials.Password, false, false)).ReturnsAsync(SignInResult.Failed);

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object, fakeSignInManager.Object, _tokenService);

            //Act&&Assert
            await Assert.ThrowsAsync<AppException>(async () => await userService.SignIn(credentials));
        }

        #endregion

        #region Register

        [Fact]
        public async Task RegisterSuccess()
        {
            //Arrange
            var userRegisterDto = new UserRegisterDto()
                { Email = "test@gmail.com", FirstName = "Test", LastName = "Testowy", UserName = "TestowyUser", Password = "Test!123@"};
            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var fakeSignInManager = new Mock<FakeSignInManager>();
            fakeSignInManager.Setup(m => m.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object, fakeSignInManager.Object, _tokenService);

            //Act

            var userLoginDtoResult = await userService.Register(userRegisterDto);

            //Assert

            userLoginDtoResult.ShouldNotBeNull();
            userLoginDtoResult.Email.ShouldEqual("test@gmail.com");
        }

        [Fact]
        public async Task RegisterFailedCreateUser()
        {
            //Arrange
            var userRegisterDto = new UserRegisterDto()
                { Email = "test@gmail.com", FirstName = "Test", LastName = "Testowy", UserName = "TestowyUser", Password = "Test!123@" };
            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            var fakeSignInManager = new Mock<FakeSignInManager>();
            fakeSignInManager.Setup(m => m.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object, fakeSignInManager.Object, _tokenService);

            //Act&Assert
            await Assert.ThrowsAsync<AppException>(async () => await userService.Register(userRegisterDto));

        }
        #endregion

        #region GetAllUsers

        [Fact]
        public async Task GetAllUsersSuccess()
        {
            //Arrange
            var users = new List<User>
            {
                new User()
                {
                    Id = "ID1",
                    UserName = "Test1",
                    Email = "testowy1@gmail.com"
                },
                new User()
                {
                    Id = "ID2",
                    UserName = "Test2",
                    Email = "test2@gmail.com"
                }
            };

            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.Users).Returns(users.AsQueryable);

            var fakeSignInManager = new Mock<FakeSignInManager>();

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object, 
                fakeSignInManager.Object, _tokenService);

            //Action

            var result = userService.GetAllUsers(10, 1);

            //Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldEqual(2);
        }
        [Fact]
        public async Task GetAllUserPageNumberBiggerThanTotalPages()
        {
            //Arrange
            var users = new List<User>
            {
                new User()
                {
                    Id = "ID1",
                    UserName = "Test1",
                    Email = "testowy1@gmail.com"
                },
                new User()
                {
                    Id = "ID2",
                    UserName = "Test2",
                    Email = "test2@gmail.com"
                }
            };

            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.Users).Returns(users.AsQueryable);

            var fakeSignInManager = new Mock<FakeSignInManager>();

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object,
                fakeSignInManager.Object, _tokenService);

            //Action

            var result = userService.GetAllUsers(10, 2);

            //Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldEqual(0);
        }
        #endregion

        #region GetUser

        [Fact]
        public async Task GetUserSuccess()
        {
            //Arrange
            var user = new User {Id = "ID1", UserName = "Test1", Email = "test@gmail.com"};

            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            var fakeSignInManager = new Mock<FakeSignInManager>();

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object, 
                fakeSignInManager.Object, _tokenService);

            //Act
            var result = await userService.GetUser("ID1");

            //Assert
            result.ShouldNotBeNull();
            result.Id.ShouldEqual("ID1");
        }

        [Fact]
        public async Task GetUserUserNotFound()
        {
            //Arrange
            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).Throws(new UserNoFoundException());

            var fakeSignInManager = new Mock<FakeSignInManager>();

            var userService = new UserService(_logger, _mapper, fakeUserManager.Object,
                fakeSignInManager.Object, _tokenService);

            //Act&Assert
            await Assert.ThrowsAsync<UserNoFoundException>(async () => await userService.GetUser("IDNOTFOUND"));
        }

        #endregion
    }
}

