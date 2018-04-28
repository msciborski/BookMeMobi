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
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase(); ;
                //Action
                var bookDto = await fixture.FileService.GetBookForUserAsync("ID1", 1);

                //Assert
                bookDto.Id.ShouldEqual(1);
                bookDto.DeleteDate.ShouldBeNull();
                bookDto.UserId.ShouldEqual("ID1");
            }

        }

        #endregion

        #region GetBooksForUserAsync

        #endregion

        #region DeleteBook(soft)

        #endregion


    }
}


