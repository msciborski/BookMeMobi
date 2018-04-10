using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BookMeMobi2.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FacebookId { get; set; }
        public string PictureUrl { get; set; }
        public string KindleEmail { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}
