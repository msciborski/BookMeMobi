﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Models.User
{
    public class UserLoginDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string KindleEmail { get; set; }
        public bool IsVerifiedAmazonConnection { get; set; }
        public IDictionary<string, TokenResource> Tokens { get; set; } 
    }
}
