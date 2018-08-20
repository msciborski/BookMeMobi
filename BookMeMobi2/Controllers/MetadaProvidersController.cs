using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookMeMobi2.MetadataProviders.GoodReads;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using BookMeMobi2.Models;
using BookMeMobi2.MetadataProviders;
using BookMeMobi2.MetadataProviders.GoodReads.Models;
using System.Linq;

namespace BookMeMobi2.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateToken]
    [Route("api/users")]
    public class MetadaProvidersController : Controller
    {
      private readonly IBookService _bookService;
      private readonly IGoodReadsMetadataProvider _goodReadsClient;

      public MetadaProvidersController(IBookService bookService, IGoodReadsMetadataProvider goodReadsClient)
      {
        _bookService = bookService;
        _goodReadsClient = goodReadsClient;
      }

      [Produces("application/json")]
      [ProducesResponseType(typeof(PagedList<GoodReadsBookDto>), 200)]
      [ValidateUserExists]
      [ValidateBookExists]
      [HttpGet("{userId}/books/{bookId}/goodreads")]
      public async Task<IActionResult> GetGoodReadsMetada(string userId, int bookId, int pageNumber = 1, int pageSize = 10)
      {
        var book = await _bookService.GetBookForUserAsync(userId, bookId);
        var booksFromGoodReads = await _goodReadsClient.GetBooks(book.Title, book.Author);
        return Ok(new PagedList<GoodReadsBookDto>(booksFromGoodReads.AsQueryable(), pageNumber, pageSize));
      }
    }
}