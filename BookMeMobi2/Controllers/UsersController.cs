using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookMeMobi2.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly JWTSettings _options;

        public UsersController(IMapper mapper, SignInManager<User> signInManager, 
            UserManager<User> userManager, IOptions<JWTSettings> options, ApplicationDbContext context)
        {
            _mapper = mapper;
            _signInManager = signInManager;
            _userManager = userManager;
            _options = options.Value;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> SignIn([FromBody] Credentials credentials)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(credentials.Username, credentials.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(credentials.Username);
                    var token = CreateToken(user);

                    return Ok(new
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Token = token
                    });
                }
                return new JsonResult("Unable to sign in.") { StatusCode = 401 };
            }
            return Error("Unexpected error occured.");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<User>(userDto);
                var result = await _userManager.CreateAsync(user, userDto.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    var token = CreateToken(user);
                    return Ok(new
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Token = token
                    });
                }
                return Errors(result);
            }
            return Error("Unexpected error occured.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Ok();
        }
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            return Ok(_context.Users.Select(u =>
                new UserDto() {Email = u.Email, FirstName = u.FirstName, LastName = u.LastName, UserName = u.UserName}));

        }



        private string CreateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_options.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
        private JsonResult Errors(IdentityResult result)
        {
            var items = result.Errors.Select(e => e.Description).ToArray();
            return new JsonResult(items) { StatusCode = 400 };
        }

        private JsonResult Error(string message)
        {
            return new JsonResult(message) { StatusCode = 400 };
        }
    }
}
