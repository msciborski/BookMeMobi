﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Models;
using BookMeMobi2.Models.Book;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookMeMobi2.Controllers
{
    [Route("/api/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BookController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly IBookService _bookService;
        private readonly IMailService _mailService;
        private readonly IUserService _userService;
        private readonly IStorageService _storageService;

        public BookController(IMapper mapper, IBookService bookService, ILogger<BookController> logger, IStorageService storageService)
        {
            _mapper = mapper;
            _logger = logger;
            _bookService = bookService;
            _storageService = storageService;
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
        [ValidateUserExists]
        [HttpGet("{userId}/books")]
        public async Task<IActionResult> GetBooks(string userId, [FromQuery] BooksResourceParameters parameters)
        {
            List<BookDto> booksDto = new List<BookDto>();
            var books = await _bookService.GetBooksForUserAsync(userId, parameters);

            foreach (var book in books)
            {
                var coverUrl = (book.Cover != null) ? _storageService.GetCoverUrl(userId, book.Id, book.Cover.CoverName) : null;
                var bookDto = _mapper.Map<Book, BookDto>(book);
                bookDto.CoverUrl = coverUrl;
                booksDto.Add(bookDto);
            }

            var pagedList = new PagedList<BookDto>(booksDto.AsQueryable(), parameters.PageNumber, parameters.PageSize);
            return Ok(pagedList);
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
        [ValidateUserExists]
        [ValidateBookExists]
        [HttpGet("{userId}/books/{bookId}")]
        public async Task<IActionResult> GetBook(string userId, int bookId)
        {
            Book book = await _bookService.GetBookForUserAsync(userId, bookId);

            var coverUrl = (book.Cover != null) ? _storageService.GetCoverUrl(userId, bookId, book.Cover.CoverName) : null;

            var bookDto = _mapper.Map<Book, BookDto>(book);
            bookDto.CoverUrl = coverUrl;

            return Ok(bookDto);
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
        [ValidateUserExists]
        [ValidateBookExists]
        [HttpDelete("{userId}/books/{bookId}")]
        public async Task<IActionResult> DeleteBook(string userId, int bookId)
        {
            var book = await _bookService.DeleteBookAsync(userId, bookId);
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
        [ValidateUserExists]
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

                var book = await _bookService.UploadBookAsync(file, userId);
                var coverUrl = (book.Cover != null)
                    ? _storageService.GetCoverUrl(userId, book.Id, book.Cover.CoverName)
                    : null;

                var bookDto = _mapper.Map<Book, BookDto>(book);
                bookDto.CoverUrl = coverUrl;

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
        [Produces("application/json")]
        [ProducesResponseType(typeof(DownloadBookDto), 200)]
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 500)]
        [ValidateUserExists]
        [ValidateBookExists]
        [HttpGet("{userId}/books/{bookId}/download")]
        public async Task<IActionResult> DownloadBook(string userId, int bookId)
        {
            Book book = await _bookService.GetBookForUserAsync(userId, bookId);
            var downloadDto = new DownloadBookDto() { Id = book.Id, Url = _bookService.GetDownloadUrl(userId, bookId, book.FileName) };
            return Ok(downloadDto);
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiError), 404)]
        [ProducesResponseType(typeof(ApiError), 500)]
        [ValidateUserExists]
        [ValidateBookExists]
        [HttpGet("{userId}/books/{bookId}/send")]
        public async Task<IActionResult> SendBook(string userId, int bookId)
        {
            await _bookService.SendBook(userId, bookId);

            return Ok();
        }

    }
}
