using BookMeMobi2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookMeMobi2.Helpers;
using BookMeMobi2.Helpers.Extensions;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class LINQExtensionTest
    {
        private readonly IEnumerable<Book> _books;

        public LINQExtensionTest()
        {
            _books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Author = "Adam Mickiewicz",
                    FileName = "dziady.mobi",
                    Title = "Dziady",
                    Format = "mobi",
                    PublishingDate = DateTime.Now,
                    Size = 0.540,
                },
                new Book
                {
                    Id = 2,
                    Author = "Bolesław Prus",
                    FileName = "lalka.mobi",
                    Title = "Lalka",
                    Format = "mobi",
                    PublishingDate = DateTime.Now.AddDays(5),
                    Size = 0.666,
                },
                new Book
                {
                    Id = 3,
                    Author = "Bolesław Prus",
                    FileName = "krzyzacy.mobi",
                    Title = "Krzyżacy",
                    Format = "mobi",
                    PublishingDate = DateTime.Now,
                    Size = 0.540,
                    IsDeleted = true,
                    DeleteDate = DateTime.Now.AddDays(6)
                },
                new Book
                {
                    Id = 4,
                    Author = "Nie chce mi sie wymyslac",
                    FileName = "test.mobi",
                    Title = "Nicosc",
                    Format = "mobi",
                    PublishingDate = DateTime.Now.AddDays(4),
                    Size = 0.540,
                },
                new Book
                {
                    Id = 5,
                    Author = "AAAAAA",
                    FileName = "AAAA.mobi",
                    Title = "AAAA",
                    Format = "mobi",
                    PublishingDate = DateTime.Now.AddDays(7),
                    Size = 0.540,
                }
            };
        }

        [Fact]
        public void FilterExtensionMethodDeleteFalseShouldReturnThree()
        {
            var result = _books.FilterBooks(new BooksResourceParameters());
            result.Count().ShouldEqual(4);
        }

        [Fact]
        public void FilterExtensionMethodDeleteTrueShouldReturnOne()
        {
            var result = _books.FilterBooks(new BooksResourceParameters() {Deleted = true});
            result.Count().ShouldEqual(1);
        }

        [Fact]
        public void SearchExtensionMethodAuthor()
        {
            var result = _books.SearchBook("MiCkieWicz");
            result.Count().ShouldEqual(1);
        }

        [Fact]
        public void SearchExtensionMethodTitle()
        {
            var result = _books.SearchBook("lalka");
            result.Count().ShouldEqual(1);
        }

        [Fact]
        public void OrderByTitleAscending()
        {
            var result = OrderBy("Title");

            result.First().Title.ShouldEqual("AAAA");
        }

        [Fact]
        public void OrderByTitleDescending()
        {
            var result = OrderBy("Title desc");

            result.First().Title.ShouldEqual("Nicosc");
        }

        [Fact]
        public void OrderByAuthorAscending()
        {
            var result = OrderBy("Author");

            result.First().Title.ShouldEqual("AAAA");
        }

        [Fact]
        public void OrderByAuthorDescending()
        {
            var result = OrderBy("Author desc");

            result.First().Title.ShouldEqual("Nicosc");
        }

        [Fact]
        public void OrderByIdAscending()
        {
            var result = OrderBy("Id");

            result.First().Title.ShouldEqual("Dziady");
        }

        [Fact]
        public void OrderByIdDescending()
        {
            var result = OrderBy("Id desc");

            result.First().Title.ShouldEqual("AAAA");
        }

        [Fact]
        public void OrderByFileNameAscending()
        {
            var result = OrderBy("FileName");

            result.First().Title.ShouldEqual("AAAA");
        }

        [Fact]
        public void OrderByFileNameDescending()
        {
            var result = OrderBy("FileName desc");

            result.First().Title.ShouldEqual("Nicosc");
        }

        [Fact]
        public void OrderByPublishingDateAscending()
        {
            var result = OrderBy("PublishingDate");

            result.First().Title.ShouldEqual("Dziady");
        }
        [Fact]
        public void OrderByPublishingDateDescending()
        {
            var result = OrderBy("PublishingDate desc");

            result.First().Title.ShouldEqual("AAAA");
        }

        [Fact]
        public void OrderByEmptyOrderByParameterShouldReturnSource()
        {
            var result = OrderBy(null);
            result.Count().ShouldEqual(5);
        }
        [Fact]
        public void OrderByNullSourceShouldThrowArugmentNullException()
        {
            var propertyMappingService = new PropertyMappingService();


        }

        [Fact]
        public void OrderByNullPropertyMappingShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _books.AsQueryable().ApplySort("Title", null));
        }
        private IQueryable<Book> OrderBy(string orderBy)
        {
            var propertyMappingService = new PropertyMappingService();

            return _books.AsQueryable().ApplySort(orderBy, propertyMappingService.GetPropertyMapping<BookDto, Book>());
        }

    }
}
