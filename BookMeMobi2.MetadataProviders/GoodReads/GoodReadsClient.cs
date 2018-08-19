using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using BookMeMobi2.MetadataProviders.GoodReads.Models;
using BookMeMobi2.MetadataProviders.Utils;

namespace BookMeMobi2.MetadataProviders.GoodReads
{
    public class GoodReadsClient : IBookMetadaProvider
    {
        private readonly IDictionary<string, string> _endpoints = new Dictionary<string, string>
        {
          { "getBooks", "https://www.goodreads.com/search/index.xml?q={0}&key={1}" }
        };
        private GoodReadsSettings _settings;

        public GoodReadsClient(GoodReadsSettings settings)
        {
            _settings = settings;
        }
        public async Task<IEnumerable<GoodReadsBookDto>> GetBooks(string title, string author, string isbn)
        {
            if (!String.IsNullOrEmpty(isbn))
            {
                var requestUri = String.Format(_endpoints["getBooks"], isbn, _settings.Key);
                var response = await SendRequest(requestUri, HttpMethod.Get);
                var books = await ProcessHttpResponse(response);
                if(books.Any())
                {
                  return books;
                }
            }
            if (!String.IsNullOrEmpty(title))
            {
                var requestUri = String.Format(_endpoints["getBooks"], title, _settings.Key);
                var response = await SendRequest(requestUri, HttpMethod.Get);
                var books = await ProcessHttpResponse(response);
                if(books.Any())
                {
                  return books;
                }
            }
            if (!String.IsNullOrEmpty(author))
            {
                var requestUri = String.Format(_endpoints["getBooks"], author, _settings.Key);
                var response = await SendRequest(requestUri, HttpMethod.Get);
                var books = await ProcessHttpResponse(response);
                if(books.Any())
                {
                  return books;
                }
            }
            return Enumerable.Empty<GoodReadsBookDto>();
        }
        private async Task<IEnumerable<GoodReadsBookDto>> ProcessHttpResponse(HttpResponseMessage response)
        {
          var content = await response.Content.ReadAsStringAsync();
          XDocument xDoc = XDocument.Parse(content);
          var goodReadsBook = xDoc.Root.Descendants("work")
                              .Select((b) => {
                                var year = String.IsNullOrEmpty(b.Element("original_publication_year").Value) ? null : b.Element("original_publication_year").Value;
                                var month = String.IsNullOrEmpty(b.Element("original_publication_month").Value) ? "1" : b.Element("original_publication_month").Value;
                                var day = String.IsNullOrEmpty(b.Element("original_publication_day").Value) ? "1" : b.Element("original_publication_day").Value;

                                return new GoodReadsBookDto
                                {
                                  PublishingDate = String.IsNullOrEmpty(year) ? (DateTime?) null : new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)),
                                  Title = b.Element("best_book").Element("title").Value,
                                  Author = b.Element("best_book").Element("author").Element("name").Value
                                };
                              });
          return goodReadsBook;
        }
        private async Task<HttpResponseMessage> SendRequest(string uri, HttpMethod method)
        {
            if (method == HttpMethod.Get)
            {
                return await HttpRequestFactory.Get(uri, "application/xml");
            }
            throw new ArgumentException("Invalid HttpMethod");
        }


    }
}