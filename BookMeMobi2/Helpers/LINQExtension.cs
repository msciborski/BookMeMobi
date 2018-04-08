using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using System.Linq.Dynamic.Core;
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

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException("mappingDictionary");
            }

            if (String.IsNullOrWhiteSpace(orderBy) || String.IsNullOrEmpty(orderBy))
            {
                return source;
            }

            var orderByAfterSplit = orderBy.Split(',');

            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                var trimmedOrderByClause = orderByClause.Trim();

                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1
                    ? trimmedOrderByClause
                    : trimmedOrderByClause.Remove(indexOfFirstSpace);

                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing.");
                }

                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    source = source.OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
                }
            }

            return source;
        }
    }
}
