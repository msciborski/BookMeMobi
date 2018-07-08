using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using BookMeMobi2.Models.Book;

namespace BookMeMobi2.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _booksPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new PropertyMappingValue(new List<string>(){"Id"}) },
            { "Title", new PropertyMappingValue(new List<string>(){"Title"}) },
            { "Author", new PropertyMappingValue(new List<string>(){"Author"}) },
            { "FileName", new PropertyMappingValue(new List<string>(){"FileName"}) },
            { "PublishingDate", new PropertyMappingValue(new List<string>(){"PublishingDate"}) },
            { "UploadDate", new PropertyMappingValue(new List<string>(){"UploadDate"}) },
            { "Size", new PropertyMappingValue(new List<string>(){"Size"}) }
        };

        private Dictionary<string, PropertyMappingValue> _tagPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
          { "Id", new PropertyMappingValue(new List<string>(){"Id"}) },
          { "TagName", new PropertyMappingValue(new List<string>() {"TagName"}) },
          { "CountUsage", new PropertyMappingValue(new List<string>() {"CountUsage"}) }
        };

        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<BookDto, Book>(_booksPropertyMapping));
            _propertyMappings.Add(new PropertyMapping<TagDto, Tag>(_tagPropertyMapping));
        }
        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First().MappingDictionary;
            }
            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}");
        }
    }
}
