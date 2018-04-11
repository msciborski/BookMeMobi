﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BookMeMobi2.Models
{
    public class BooksResourceParameters
    {
        private const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        public bool Deleted { get; set; } = false;
        public string SearchQuery { get; set; }
        public string OrderBy { get; set; } = "UploadDate desc";

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
