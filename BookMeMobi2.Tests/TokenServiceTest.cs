using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class TokenServiceTest
    {
        private readonly IOptions<JWTSettings> _options;
        public TokenServiceTest()
        {
            var jwtSettings = new JWTSettings() {Secret = "Dumb as hell secret, only for test purpose, i love ya so much !123"};
            _options = Microsoft.Extensions.Options.Options.Create(jwtSettings);
        }

        [Fact]
        public async Task CreateToken()
        {
            //Arrange
            var user = new User {Id = "ID1", UserName = "Test1"};
            var tokenService = new TokenService(_options);

            //Act
            var token = tokenService.CreateToken(user.Id);

            //Assert
            // token.Should().NotBeNullOrEmpty().And.ShouldNotBeType<String>();
        }
    }
}
