using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;

namespace BookMeMobi2.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
