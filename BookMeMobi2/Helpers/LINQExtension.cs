using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;

namespace BookMeMobi2.Helpers
{
    public static class LINQExtension
    {
        public static IEnumerable<Book> FilterBooks(this IEnumerable<Book> source, BooksResourceParameters parameters)
        {
            return source.Where(b => b.IsDeleted == parameters.Deleted);
        }

        public static IEnumerable<Book> SearchBook(this IEnumerable<Book> source, string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery) || String.IsNullOrWhiteSpace(searchQuery))
            {
                return source;
            }

            searchQuery = searchQuery.ToLowerInvariant();

            return source.Where(b =>
                b.Author.ToLowerInvariant().Contains(searchQuery) || b.Title.ToLowerInvariant().Contains(searchQuery) || b.FileName.ToLowerInvariant().Contains(searchQuery));
        }
    }
}
