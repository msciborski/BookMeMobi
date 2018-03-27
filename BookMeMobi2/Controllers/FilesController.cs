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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly IStorageService _storageService;

        public FilesController(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager, IStorageService storageService, ILogger<FilesController> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _storageService = storageService;
        }

        [HttpGet("{userId}/books")]
        public async Task<IActionResult> GetBooks(string userId, [FromQuery(Name = "page_size")] int pageSize = 10, [FromQuery(Name = "page_number")] int pageNumber = 1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.Include(u => u.Books).FirstOrDefaultAsync(u => u.Id.Equals(userId));

            if (user == null)
            {
                return NotFound($"User {userId} not found");
            }

            var listOfBookDto = _mapper.Map<IEnumerable<Book>, IEnumerable<BookDto>>(user.Books);

            PagedList<BookDto> pagedList = new PagedList<BookDto>(listOfBookDto.AsQueryable(), pageNumber, pageSize);

            return Ok(pagedList);
        }

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
                book = await GetBookForUser(userId, bookId);
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

        [HttpPost("{userId}/books")]
        public async Task<IActionResult> UploadMobiFile([FromForm] IFormCollection fileCollection, string userId)
        {
            List<BookDto> files = new List<BookDto>();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError($"User {userId} dosen't exist.");
                return BadRequest();
            }
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

                    var bookDto = await _storageService.UploadBook(file, user);
                    files.Add(bookDto);

                    _logger.LogInformation($"{bookDto.FullName} is uploaded.");
                }
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
            return Ok(files);
        }


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
                book = await GetBookForUser(userId, bookId);
                var stream = await _storageService.DownloadBook(book);
                stream.Position = 0;
                var name = $"{book.FullName}.mobi";
                var result = File(stream, "application/x-mobipocket-mobi", name);
                return result;
            }
            catch (AppException e)
            {
                _logger.LogError(e.Message);

                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                return BadRequest(e.Message);
            }

        }

        private async Task<Book> GetBookForUser(string userId, int bookId)
        {
            var user = await _context.Users.Include(u => u.Books).FirstOrDefaultAsync(u => u.Id.Equals(userId));
            if (user == null)
            {
                _logger.LogError($"User {userId} dosen't exist.");
                throw new UserNoFoundException($"User {userId} no found.");
            }

            var book = user.Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                _logger.LogError($"Book {bookId} dosen't exist.");
                throw new BookNoFoundException($"Book {bookId} no found.");
            }

            return book;
        }

    }
}
