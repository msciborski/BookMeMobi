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

namespace BookMeMobi2.Controllers
{
    [Route("/api/users")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FilesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly IStorageService _storageService;

        public FilesController(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager, IStorageService storageService)
        {
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
            _storageService = storageService;
        }

        [HttpPost("{userId}/files")]
        public async Task<IActionResult> UploadMobiFile([FromForm] IFormCollection fileCollection, string userId)
        {
            List<BookDto> files = new List<BookDto>();
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest();
            }

            try
            {
                foreach (var file in fileCollection.Files)
                {
                    if(file.Length == 0)
                    {
                        throw new Exception("File is empty.");
                    }

                    var bookDto = await _storageService.UploadBook(file, user);
                    files.Add(bookDto);
                }
            }
            catch (Exception e)
            {
                return new JsonResult(Enumerable.Empty<BookDto>()) { StatusCode = 500 };
            }
            return Ok(files);
        }

    }
}
