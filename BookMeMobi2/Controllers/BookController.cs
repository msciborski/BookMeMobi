using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Fliters;
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
    public class BookController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly IBookService _fileService;
        private readonly IMailService _mailService;
        private readonly IUserService _userService;


        public BookController(IMapper mapper, IBookService storageService, ILogger<BookController> logger)
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
        [ProducesResponseType(typeof(ApiError), 404)]
        [ValidateModel]
        [HttpGet("{userId}/books")]
        public async Task<IActionResult> GetBooks(string userId, [FromQuery] BooksResourceParameters parameters)
        {
            PagedList<BookDto> booksDto = await _fileService.GetBooksForUserAsync(userId, parameters);
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
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 500)]
        [ValidateModel]
        [HttpGet("{userId}/books/{bookId}")]
        public async Task<IActionResult> GetBook(string userId, int bookId)
        {
            Book book = await _fileService.GetBookForUserAsync(userId, bookId);

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
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 500)]
        [HttpDelete("{userId}/books/{bookId}")]
        public async Task<IActionResult> DeleteBook(string userId, int bookId)
        {
            var book = await _fileService.DeleteBookAsync(userId, bookId);
            var bookDto = _mapper.Map<Book, BookDeleteDto>(book);
            return Ok(bookDto);
        }

        /// <summary>
        /// Upload book
        /// </summary>
        /// <param name="fileCollection">File from form</param>
        /// <param name="userId">User id</param>
        /// <returns>Uploaded book</returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), 200)]
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 500)]
        [ValidateModel]
        [HttpPost("{userId}/books")]
        public async Task<IActionResult> UploadMobiFile([FromForm] IFormCollection fileCollection, string userId)
        {
            List<BookDto> files = new List<BookDto>();
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
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 500)]
        [HttpGet("{userId}/books/{bookId}/download")]
        public async Task<IActionResult> DownloadBook(string userId, int bookId)
        {
            Book book = await _fileService.GetBookForUserAsync(userId, bookId);
            var stream = await _fileService.DownloadBookAsync(book);
            stream.Position = 0;
            var result = File(stream, "application/x-mobipocket-mobi", book.FileName);
            return result;
        }

        [HttpGet("{userId}/books/{bookId}/send")]
        public async Task<IActionResult> SendBook(string userId, int bookId)
        {
            await _fileService.SendBook(userId, bookId);

            return Ok();
        }

    }
}
