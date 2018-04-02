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
        private readonly UserServiceTestFixture<Startup> _fixture;
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


        [Fact]
        public async Task SignInUserSuccess()
        {
            //Arrange
            var credentials = new Credentials() {Password = "SłabeHasło!123", Username = "TestUserName"};
            var user = new User() {Id = "ID1", UserName = "TestUserName"};

            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(m => m.FindByNameAsync(credentials.Username)).ReturnsAsync(user);

            var fakeSignInManager = new Mock<FakeSignInManager>();
            fakeSignInManager.Setup(
                m => m.PasswordSignInAsync(credentials.Username, credentials.Password, false, false)).ReturnsAsync(SignInResult.Success);

            var userService = new UserService(_logger,_mapper,fakeUserManager.Object,fakeSignInManager.Object,_tokenService);

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
    }
}

