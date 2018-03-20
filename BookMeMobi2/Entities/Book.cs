﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string FullName { get; set; }
        public DateTime? PublishingDate { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}