using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.MobiMetadata;
using BookMeMobi2.Models;
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

        public FilesController(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager)
        {
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
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
                    using(var stream = file.OpenReadStream())
                    {
                        var bookDto = GetMobiMetadata(stream);

                    }
                    //var bookDto = GetMobiMetadata(file);
                    //AddFilesToDb(bookDto, userId);
                    //files.Add(bookDto);

                }
            }
            catch (Exception e)
            {
                return new JsonResult(Enumerable.Empty<BookDto>()) { StatusCode = 500 };
            }
            return Ok(files);
        }

        private void AddFilesToDb(BookDto bookDto, string userId)
        {
            var book = _mapper.Map<BookDto, Book>(bookDto);
            _context.Books.Add(book);
            _context.SaveChanges();
            bookDto.Id = book.Id;
        }

        private string UploadFileToFtp(IFormFile file)
        {
            //There will be code which will be suposed to upload file to ftp server
            //Method returns url for file
            return "http://testowo.pl/ftp/";
        }

        public BookDto GetMobiMetadata(Stream stream)
        {
            BookDto fileDto = new BookDto();
            var mobiDocument = MobiService.LoadDocument(stream);
            fileDto.Author = mobiDocument.Author;
            fileDto.Title = mobiDocument.Title;
            fileDto.PublishingDate = mobiDocument.PublishingDate;
            fileDto.FullName = mobiDocument.MobiHeader.FullName;
            return fileDto;
        }

    }
}
