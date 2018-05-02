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
        Task<UserLoginDto> SignIn(Credentials credentials);
        Task<User> Register(UserRegisterDto userDto);
        Task ConfirmEmail(string userId, string token);
        Task ForgotPassword(string userName);
        Task ResetPassword(string userId, UserResetPasswordDto model);
        Task Logout();
        PagedList<UserDto> GetAllUsers(int pageSize, int pageNumber);
        Task<User> GetUser(string userId);
        Task UpdateUserAsync(string userId, UserUpdateDto model);

    }
}
