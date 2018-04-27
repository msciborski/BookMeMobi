﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;

namespace BookMeMobi2.Services
{
    public interface IStorageService
    {
        Task UploadBookAsync(Stream file, string userId, int bookId, string bookFileName);
        Task<Stream> DownloadBookAsync(string userId, int bookId, string bookFileName);
        string GetDownloadUrl(string userId, int bookId, string bookFileName);
    }
}