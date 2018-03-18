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
        public FilesController(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpPost("{userId}/files")]
        public async Task<IActionResult> UploadMobiFile(IFormCollection fileCollection, [FromBody] string userId)
        {
            List<BookDto> files = new List<BookDto>();

            try
            {
                foreach (var file in fileCollection.Files)
                {
                    var fileDto = GetMobiMetadata(file);
                    fileDto.DownloadUrl = UploadFileToFtp(file);
                    files.Add(fileDto);
                }
                AddFilesToDb(files, userId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}, {e.StackTrace}");
                return new JsonResult(Enumerable.Empty<BookDto>()) { StatusCode = 500 };
            }

            return Ok(files);
        }

        private void AddFilesToDb(IEnumerable<BookDto> files, string userId)
        {
            var books = _mapper.Map<IEnumerable<BookDto>, IEnumerable<Book>>(files);

            foreach (var book in books)
            {
                book.UserId = userId;
                _context.Books.Add(book);
            }
            _context.SaveChanges();

        }

        private string UploadFileToFtp(IFormFile file)
        {
            //There will be code which will be suposed to upload file to ftp server
            //Method returns url for file
            return "http://testowo.pl/ftp/";
        }

        public BookDto GetMobiMetadata(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

            BookDto fileDto = new BookDto();
            using (Stream stream = file.OpenReadStream())
            {
                var mobiDocument = MobiService.LoadDocument(stream);
                fileDto.Author = mobiDocument.Author;
                fileDto.Title = mobiDocument.Title;
                fileDto.PublishingDate = mobiDocument.PublishingDate;
                fileDto.FullName = mobiDocument.MobiHeader.FullName;

            }
            return fileDto;
        }

    }
}
