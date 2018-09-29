using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using BookMeMobi2.Models.Book;
using BookMeMobi2.Services;

namespace BookMeMobi2.Helpers.Extensions
{
    public static class LINQExtension
    {
        public static IEnumerable<Book> FilterBooks(this IEnumerable<Book> source, BooksResourceParameters parameters)
        {
            source = source.Where(b => b.IsDeleted == parameters.Deleted);

            if (parameters.SentKindle.HasValue)
            {
                source = source.Where(b => b.IsSentToKindle == parameters.SentKindle.Value);
            }
            if (parameters.IsPublic.HasValue)
            {
                source = source.Where(b => b.IsPublic == parameters.IsPublic.Value);
            }

            return source;
        }
        public static IEnumerable<Book> FilterBooksByTags(this IEnumerable<Book> source, IEnumerable<string> tags)
        {
          if (tags != null && tags.Any())
          {
            var booksWithTags = source.Where(b => tags.Except(b.BookTags.Select(bt => bt.Tag.TagName)).Any());
            return booksWithTags;
          }
          return source;
        }

        public static IEnumerable<Book> SearchBook(this IEnumerable<Book> source, string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery) || String.IsNullOrWhiteSpace(searchQuery))
            {
                return source;
            }

            searchQuery = searchQuery.ToLowerInvariant();

            return source.Where(b =>
                b.Author.ToLowerInvariant().Contains(searchQuery) ||
                  b.Title.ToLowerInvariant().Contains(searchQuery) ||
                    b.FileName.ToLowerInvariant().Contains(searchQuery));
        }
        public static IEnumerable<Tag> SearchTag(this IEnumerable<Tag> source, string tagName)
        {
            if (String.IsNullOrEmpty(tagName) || String.IsNullOrWhiteSpace(tagName))
            {
                return source;
            }

            tagName = tagName.ToLowerInvariant();

            return source.Where(t => t.TagName.ToLowerInvariant().Equals(tagName));
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
                    return source;
                }

                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue == null)
                {
                    return source;
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
