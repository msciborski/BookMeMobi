using System.Collections.Generic;
using System.Threading.Tasks;
using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookMeMobi2.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ValidateToken]
    public class TagController : Controller
    {

        private readonly ITagService _tagService;
        public TagController(ITagService tagService)
        {
          _tagService = tagService;
        }

        [ValidateUserExists]
        [ValidateBookExists]
        [HttpGet("/api/users/{userId}/books/{bookId}/tags")]
        public async Task<IActionResult> GetBookTags(string userId, int bookId)
        {
            var bookTags = _tagService.GetBookTags(bookId);
            return Ok(bookTags);
        }

        [ValidateUserExists]
        [ValidateBookExists]
        [HttpPost("/api/users/{userId}/books/{bookId}/tags")]
        public async Task<IActionResult> AddBookTags(string userId, int bookId, [FromBody] IEnumerable<string> tagNames)
        {
            await _tagService.AddTagsToBook(bookId, tagNames);

            return NoContent();
        }




    }
}