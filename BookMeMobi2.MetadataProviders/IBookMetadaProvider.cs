using System.Collections.Generic;
using System.Threading.Tasks;
using BookMeMobi2.MetadataProviders.GoodReads.Models;

namespace BookMeMobi2.MetadataProviders
{
    public interface IBookMetadaProvider
    {
      Task<IEnumerable<GoodReadsBookDto>> GetBooks(string title, string author, string isbn);
    }
}