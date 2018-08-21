using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BookMeMobi2.Models.Book
{
    public class BooksResourceParameters : ResourceParametersBase
    {

        public BooksResourceParameters()
        {
          base.OrderBy = "UploadDate desc";
        }
        public string[] Tags { get; set; }
        public bool Deleted { get; set; } = false;
        public string SearchQuery { get; set; }
        public bool? SentKindle { get; set; } = null;

    }
}
