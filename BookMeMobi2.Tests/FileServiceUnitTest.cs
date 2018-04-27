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
                var bookDto = await fixture.FileService.GetBookForUserAsync("ID1", 1, false);

                //Assert
                bookDto.Id.ShouldEqual(1);
                bookDto.DeleteDate.ShouldBeNull();
                bookDto.UserId.ShouldEqual("ID1");
            }

        }

        [Fact]
        public async Task ThrowBookNoFoundExceptionForInvalidBookId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();

                //Action&Assert
                await Assert.ThrowsAsync<BookNoFoundException>(async () =>
                    await fixture.FileService.GetBookForUserAsync("ID1", 11111, false));
            }
        }

        [Fact]
        public async Task ThrowUserNoFoundExceptionForInvalidUserId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();

                //Action&Assert
                await Assert.ThrowsAsync<UserNoFoundException>(async () =>
                    await fixture.FileService.GetBookForUserAsync("ID1000", 1, false));
            }


        }
        #endregion

        #region GetBooksForUserAsync

        [Fact]
        public async Task ReturnBooksForUserId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();
                //Action&Assert

                await Assert.ThrowsAsync<UserNoFoundException>(async () =>
                    await fixture.FileService.GetBooksForUserAsync("ID101", new BooksResourceParameters()));
            }


        }

        [Fact]
        public async Task GetBooksForUserIdInvalidUserId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();
                //Action

                var result = await fixture.FileService.GetBooksForUserAsync("ID1", new BooksResourceParameters());

                //Assert

                result.Items.Count.ShouldEqual(2);
                result.HasNextPage.ShouldEqual(false);
                result.PageSize.ShouldEqual(10);
            }
        }

        #endregion

        #region DeleteBook(soft)

        [Fact]
        public async Task DeleteBookForValidUserIdAndBookId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();

                //Action
                var result = await fixture.FileService.DeleteBookAsync("ID1", 1);

                //Assert
                result.Id.ShouldEqual(1);
                var booksForUser = await fixture.FileService.GetBooksForUserAsync("ID1", new BooksResourceParameters());
                booksForUser.Items.Count.ShouldEqual(1);
            }
        }

        [Fact]
        public async Task DeleteBookForInvalidUserId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();

                //Action&&Assert

                await Assert.ThrowsAsync<UserNoFoundException>(
                    async () => await fixture.FileService.DeleteBookAsync("ID11111", 1));
            }
        }

        [Fact]
        public async Task DeleteBookForInvalidBookId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();

                //Action&&Assert

                await Assert.ThrowsAsync<BookNoFoundException>(
                    async () => await fixture.FileService.DeleteBookAsync("ID1", 1111111));
            }
        }

        [Fact]
        public async Task DeleteBookForInvalidUserIdAndBookId()
        {
            using (var fixture = new FileServiceTestFixture())
            {
                //Arrange
                await fixture.SeedDatabase();

                //Action&&Assert

                await Assert.ThrowsAsync<UserNoFoundException>(
                    async () => await fixture.FileService.DeleteBookAsync("ID111111", 1111111));
            }
        }

        #endregion


    }
}


