using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.MobiMetadata;
using BookMeMobi2.Models;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        private readonly IStorageService _storageService;

        public FilesController(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager, IStorageService storageService, ILogger<FilesController> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _storageService = storageService;
        }

        [HttpPost("{userId}/files")]
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
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception occured.");

                return new JsonResult(Enumerable.Empty<BookDto>()) { StatusCode = 500 };
            }
            return Ok(files);
        }

    }
}
