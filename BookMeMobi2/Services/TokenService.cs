using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BookMeMobi2.Helpers.Extensions;

namespace BookMeMobi2.Services
{
    public class TokenService : ITokenService
    {
        private readonly JWTSettings _options;

        public TokenService(IOptions<JWTSettings> options)
        {
            _options = options.Value;
        }

        public TokenResource CreateToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_options.Secret);
            var expiry = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiry);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId)
                }),
                Expires = expiry,
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new TokenResource {Token = tokenString, Expiry = expiry.ToUnixTimeStamp()} ;
        }
        
        public TokenResource CreateRefreshToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            var key = Encoding.ASCII.GetBytes(_options.Secret);
            var expiry = now.AddMinutes(_options.RefreshTokenExpiry);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId)
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new TokenResource { Token = tokenString, Expiry = expiry.ToUnixTimeStamp()};
        }
        public bool ValidateRefreshToken(string userId, string refreshToken)
        {
            var decodedToken = DecodeToken(refreshToken);
            var payload = decodedToken.Payload;

            if(payload.ContainsKey("unique_name") && payload.ContainsKey("exp"))
            {
                payload.TryGetValue("unique_name", out var payloadUserId);
                var expiry = payload.Exp;
                var unixTimeStampNow = DateTime.UtcNow.ToUnixTimeStamp();
                var differenceBetweenExpiryAndNow = expiry - unixTimeStampNow;
                if(userId.Equals((string)payloadUserId) && differenceBetweenExpiryAndNow > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public JwtSecurityToken DecodeToken(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var decodedToken = jwtHandler.ReadJwtToken(token);

            return decodedToken;
        }


    }
}
