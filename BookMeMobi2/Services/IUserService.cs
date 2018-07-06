using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models.User;
using BookMeMobi2.Models;

namespace BookMeMobi2.Services
{
    public interface IUserService
    {
        Task<UserLoginDto> SignInAsync(Credentials credentials);
        Task<User> RegisterAsync(UserRegisterDto userDto);
        Task ConfirmEmailAsync(string userId, string token);
        Task ForgotPasswordAsync(string userName);
        Task ResetPasswordAsync(string userId, UserResetPasswordDto model);
        Task LogoutAsync();
        PagedList<UserDto> GetAllUsers(int pageSize, int pageNumber);
        Task<User> GetUserAsync(string userId);
        Task UpdateUserAsync(string userId, UserUpdateDto model);
        IDictionary<string, TokenResource> RefreshToken(string userId, string refreshToken);


    }
}
