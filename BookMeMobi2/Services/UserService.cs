using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Models.User;
using BookMeMobi2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookMeMobi2.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMailService _mailService;

        public UserService(ILogger<UserService> logger, IMapper mapper, UserManager<User> userManager,
            SignInManager<User> signInManager, ITokenService tokenService, IMailService mailService)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mailService = mailService;
        }
        //[ApiException]
        public async Task<UserLoginDto> SignIn(Credentials credentials)
        {
            var user = await _userManager.FindByNameAsync(credentials.Username);
            if (user == null)
            {
                throw new UserNoFoundException($"User {credentials.Username} dosen't exist.");
            }
            if (!user.EmailConfirmed)
            {
                throw new AppException("Email is not confirmed.");
            }

            var result =
                await _signInManager.PasswordSignInAsync(credentials.Username, credentials.Password, false, false);
            if (result.Succeeded)
            {

                var accessToken = _tokenService.CreateToken(user.Id);
                var refreshToken = _tokenService.CreateRefreshToken(user.Id);
                IDictionary<string, TokenResource> tokens = new Dictionary<string, TokenResource> 
                {
                    { "accessToken", accessToken },
                    { "refreshToken", refreshToken}
                };

                var userLoginDto = _mapper.Map<User, UserLoginDto>(user);
                userLoginDto.Tokens = tokens;


                _logger.LogInformation($"{user.Id} sign in.");

                return userLoginDto;
            }
            throw new AppException(result.ToString());
        }

        public async Task<User> Register(UserRegisterDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {

                _logger.LogInformation($"Succesful register user {user.Id}");

                await SendEmailConfirmation(user);

                return user;
            }

            var errors = Errors(result);
            throw new AppException(errors);
        }

        public async Task ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new AppException("User email was confirmed.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                throw new ConfirmationEmailException(Errors(result));
            }
        }

        public async Task ForgotPassword(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                throw new UserNoFoundException($"User with username {userName} dosen't exist");
            }
            if (!(await _userManager.IsEmailConfirmedAsync(user)))
            {
                throw new AppException("Email was not confirmed.");
            }

            await SendResetPasswordEmail(user);

        }

        public async Task ResetPassword(string userId, UserResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                throw new AppException(result.ToString());
            }
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation("Sing out.");
        }

        public PagedList<UserDto> GetAllUsers(int pageSize, int pageNumber)
        {
            var users = _userManager.Users;
            var usersDto = _mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(users);
            return new PagedList<UserDto>(usersDto.AsQueryable(), pageNumber, pageSize);
        }

        public async Task<User> GetUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user;
        }

        public async Task UpdateUserAsync(string userId, UserUpdateDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);

            user.Email = !String.IsNullOrEmpty(model.EmailAddress) ? model.EmailAddress : user.Email;
            user.KindleEmail = !String.IsNullOrEmpty(model.KindleEmail) ? model.KindleEmail : user.KindleEmail;
            user.FirstName = !String.IsNullOrEmpty(model.FirstName) ? model.FirstName : user.FirstName;
            user.LastName = !String.IsNullOrEmpty(model.LastName) ? model.LastName : user.LastName;
            user.IsVerifiedAmazonConnection = model.IsVerifiedAmazonConnection ? model.IsVerifiedAmazonConnection : user.IsVerifiedAmazonConnection;

            await _userManager.UpdateAsync(user);
        }

        public async Task RefreshToken(string userId, string refreshToken)
        {
            var user = await _userManager.FindByIdAsync(userId);
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

        private async Task SendEmailConfirmation(User user)
        {
            var corfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callBackUrl = $"http://bookmemobi.tk/confirm?userId={user.Id}&token={corfirmationToken}";

            await _mailService.SendMailAsync(user.Email, "Confirmation mail", callBackUrl);
        }

        private async Task SendResetPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callBackUrl = $"http://bookmemobi.tk/resetPassword?userId={user.Id}&token={token}";

            await _mailService.SendMailAsync(user.Email, "Reset password", callBackUrl);
        }
    }
}
