using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.MobiMetadata;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookMeMobi2.Controllers
{
    [Route("/api/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FilesController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly IFileService _fileService;

        public FilesController(IMapper mapper, IFileService storageService, ILogger<FilesController> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _fileService = storageService;
        }


        /// <summary>
        /// Returns user's books. 
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="pageSize">Page size, defualt 10</param>
        /// <param name="pageNumber"> Page number, default: 1</param>
        /// <returns></returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedList<BookDto>), 200)]
        [ProducesResponseType(typeof(string), 404)]

        [HttpGet("{userId}/books")]
        public async Task<IActionResult> GetBooks(string userId, [FromQuery(Name = "page_size")] int pageSize = 10, [FromQuery(Name = "page_number")] int pageNumber = 1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PagedList<BookDto> booksDto = null;
            try
            {
                booksDto = await _fileService.GetBooksForUserAsync(userId, pageSize, pageNumber);

            }
            catch (UserNoFoundException e)
            {
                _logger.LogError(e.Message);
                return NotFound(e.Message);
            }
            return Ok(booksDto);
        }


        /// <summary>
        /// Returns user's book with particular id
        /// </summary>
        /// <param name="userId">Book owner Id</param>
        /// <param name="bookId">Book id</param>
        /// <returns></returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(BookDto), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string),500)]
        [HttpGet("{userId}/books/{bookId}")]
        public async Task<IActionResult> GetBook(string userId, int bookId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid.");
                return BadRequest(ModelState);
            }

            Book book = null;
            try
            {
                book = await _fileService.GetBookForUserAsync(userId, bookId);
            }
            catch (AppException e)
            {
                _logger.LogError(e.Message);

                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                return NotFound(e.Message);
            }

            return Ok(_mapper.Map<Book, BookDto>(book));
        }

        /// <summary>
        /// Delete user's book for particular id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="bookId"></param>
        /// <returns>Info of deleted book</returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(BookDto), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpDelete]
        public async Task<IActionResult> DeleteBook(string userId, int bookId)
        {
            try
            {
                var book = await _fileService.DeleteBookAsync(userId, bookId);
                var bookDto = _mapper.Map<Book, BookDeleteDto>(book);
                return Ok(bookDto);
            }
            catch (UserNoFoundException e)
            {
                _logger.LogError(e.Message);
                return NotFound(e.Message);
            }
            catch (BookNoFoundException e)
            {
                _logger.LogError(e.Message);
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"{e.Message}, {e.StackTrace}");
                return new JsonResult("Unexpected internal error.") {StatusCode = 500};
            }
        }

        /// <summary>
        /// Upload book
        /// </summary>
        /// <param name="fileCollection">File from form</param>
        /// <param name="userId">User id</param>
        /// <returns>Uploaded book</returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpPost("{userId}/books")]
        public async Task<IActionResult> UploadMobiFile([FromForm] IFormCollection fileCollection, string userId)
        {
            List<BookDto> files = new List<BookDto>();
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model State error.");
                return BadRequest();
            }

            try
            {
                foreach (var file in fileCollection.Files)
                {
                    if (file.Length == 0)
                    {
                        _logger.LogError($"File {file.FileName} length == 0");
                        return BadRequest();
                    }

                    if (!file.FileName.Contains(".mobi"))
                    {
                        _logger.LogError($"User {userId} tried to upload no mobi file.");
                        return BadRequest();
                    }

                    var bookDto = await _fileService.UploadBookAsync(file, userId);
                    files.Add(bookDto);

                    _logger.LogInformation($"{bookDto.FileName} is uploaded.");
                }
            }
            catch (UserNoFoundException e)
            {
                _logger.LogError(e.Message);
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                return BadRequest(e.Message);
            }
            return Ok(files);
        }


        /// <summary>
        /// Download user's book with particular id
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="bookId">Book id</param>
        /// <returns></returns>
        [Produces("application/x-mobipocket-mobi")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpGet("{userId}/books/{bookId}/download")]
        public async Task<IActionResult> DownloadBook(string userId, int bookId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid");
                return BadRequest();
            }

            Book book = null;
            try
            {
                book = await _fileService.GetBookForUserAsync(userId, bookId);
                var stream = await _fileService.DownloadBookAsync(book);
                stream.Position = 0;
                var result = File(stream, "application/x-mobipocket-mobi", book.FileName);
                return result;
            }
            catch (AppException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

    }
}
