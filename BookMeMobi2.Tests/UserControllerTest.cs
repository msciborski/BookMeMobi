using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Controllers;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Mappings;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Mvc;
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
    }
}
