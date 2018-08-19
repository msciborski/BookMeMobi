using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Models;
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
        private readonly IMapper _mapper;
        public TagController(ITagService tagService, IMapper mapper)
        {
            _tagService = tagService;
            _mapper = mapper;
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedList<TagDto>), 200)]
        [ProducesResponseType(401)]
        [ValidateModel]
        [HttpGet("/api/tags")]
        public IActionResult GetTags([FromQuery] TagResourceParameters parameters)
        {
            var tags = _tagService.GetTags(parameters);

            var tagsDto = _mapper.Map<IEnumerable<Tag>, IEnumerable<TagDto>>(tags);

            var pagedList = new PagedList<TagDto>(tagsDto.AsQueryable(), parameters.PageNumber, parameters.PageSize);

            return Ok(pagedList);
        }

        [Produces("application/json")]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(typeof(IEnumerable<TagDto>), 200)]
        [ValidateUserExists]
        [ValidateBookExists]
        [HttpGet("/api/users/{userId}/books/{bookId}/tags")]
        public IActionResult GetBookTags(string userId, int bookId)
        {
            var bookTags = _tagService.GetBookTags(bookId);

            var bookTagsDto = _mapper.Map<IEnumerable<Tag>, IEnumerable<TagDto>>(bookTags);

            return Ok(bookTagsDto);
        }

        [Produces("application/json")]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(204)]
        [ValidateModel]
        [ValidateUserExists]
        [ValidateBookExists]
        [HttpPost("/api/users/{userId}/books/{bookId}/tags")]
        public async Task<IActionResult> AddBookTags(string userId, int bookId, [FromBody] IEnumerable<string> tagNames)
        {
            var book = await _tagService.AddTagsToBookAsync(bookId, tagNames);

            return Ok(book);
        }

        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ValidateUserExists]
        [ValidateBookExists]
        [HttpDelete("/api/users/{userId}/books/{bookId}/tags/{tagId}")]
        public async Task<IActionResult> DeleteTagFromBook(string userId, int bookId, int tagId)
        {
          var book = await _tagService.DeleteTagFromBook(bookId, tagId);

          return Ok(book);
        }




    }
}