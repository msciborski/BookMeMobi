using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Models.User
{
    public class UserUpdateDto
    {
        public string EmailAddress { get; set; }
        public string KindleEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsVerifiedAmazonConnection { get; set; }
    }
}
