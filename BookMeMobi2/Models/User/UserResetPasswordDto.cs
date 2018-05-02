using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace BookMeMobi2.Models.User
{
    public class UserResetPasswordDto 
    {
        public string NewPassword { get; set; }
        public string Token { get; set; }
    }
}
