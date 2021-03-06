﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;

namespace BookMeMobi2.Models.Book
{
    public class BookUpdateDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime? PublishingDate { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDeleted { get; set; }
    }
}
