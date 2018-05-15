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
        public string FileName { get; set; }
        public DateTime? PublishingDate { get; set; }
        public DateTime UploadDate { get; set; }
        public double Size { get; set; }
        public string Format { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeleteDate { get; set; }
        public bool IsSentToKindle { get; set; } = false;



        public string UserId { get; set; }
        public User User { get; set; }

        public int? CoverId { get; set; }
        public Cover Cover { get; set; }
    }
}
