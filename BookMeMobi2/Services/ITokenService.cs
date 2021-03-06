﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;

namespace BookMeMobi2.Services
{
    public interface ITokenService
    {
        TokenResource CreateToken(string userId);
        TokenResource CreateRefreshToken(string userId);
        bool ValidateRefreshToken(string userId, string refreshToken);
    }
}
