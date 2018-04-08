using BookMeMobi2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookMeMobi2.Helpers;
using BookMeMobi2.Models;
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
                    PublishingDate = DateTime.Now,
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
                    PublishingDate = DateTime.Now,
                    Size = 0.540,
                },
            };
        }

        [Fact]
        public void FilterExtensionMethodDeleteFalseShouldReturnThree()
        {
            var result = _books.FilterBooks(new BooksResourceParameters());
            result.Count().ShouldEqual(3);
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

    }
}
