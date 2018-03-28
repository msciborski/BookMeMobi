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
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FilesController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly IFileService _fileService;

        public FilesController(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager, IFileService storageService, ILogger<FilesController> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _fileService = storageService;
        }

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
                booksDto = await _fileService.GetBooksForUser(userId, pageSize, pageNumber);

            }
            catch (UserNoFoundException e)
            {
                _logger.LogError(e.Message);
                return NotFound(e.Message);
            }
            return Ok(booksDto);
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
                book = await _fileService.GetBookForUser(userId, bookId);
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

                    var bookDto = await _fileService.UploadBook(file, user);
                    files.Add(bookDto);

                    _logger.LogInformation($"{bookDto.FileName} is uploaded.");
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
                book = await _fileService.GetBookForUser(userId, bookId);
                var stream = await _fileService.DownloadBook(book);
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
