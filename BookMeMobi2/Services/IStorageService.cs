using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;

namespace BookMeMobi2.Services
{
    public interface IStorageService
    {
        Task<string> UploadBook(Stream file, User user, string bookName);
    }
}
