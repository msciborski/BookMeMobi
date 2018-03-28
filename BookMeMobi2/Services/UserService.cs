using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookMeMobi2.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;

        public UserService(ILogger<UserService> logger, IMapper mapper, ApplicationDbContext context,
            UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<UserLoginDto> SignIn(Credentials credentials)
        {
            var result =
                await _signInManager.PasswordSignInAsync(credentials.Username, credentials.Password, false, false);
            if (result.Succeeded)
            {

                var user = await _userManager.FindByNameAsync(credentials.Username);
                var token = _tokenService.CreateToken(user);

                var userLoginDto = _mapper.Map<User, UserLoginDto>(user);
                userLoginDto.Token = token;

                _logger.LogInformation($"{user.Id} sign in.");

                return userLoginDto;
            }
            throw new AppException(result.ToString());
        }

        public async Task<UserLoginDto> Register(UserRegisterDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                var token = _tokenService.CreateToken(user);

                _logger.LogInformation($"Succesful register user {user.Id}");

                var userLoginDto = _mapper.Map<User, UserLoginDto>(user);
                userLoginDto.Token = token;

                return userLoginDto;
            }

            var errors = Errors(result);
            throw new AppException(errors);
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation("Sing out.");
        }

        public PagedList<User> GetAllUsers(int pageSize, int pageNumber)
        {
            var users = _context.Users;
            return new PagedList<User>(users, pageNumber, pageSize);
        }

        public async Task<User> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new UserNoFoundException($"User {userId} no found.");
            }

            return user;
        }

        private string Errors(IdentityResult result)
        {
            var items = result.Errors.Select(e => e.Description).AsEnumerable();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in items)
            {
                stringBuilder.Append($"{item}\n");
            }

            return stringBuilder.ToString();
        }

        private JsonResult Error(string message)
        {
            return new JsonResult(message) { StatusCode = 400 };
        }

    }
}
