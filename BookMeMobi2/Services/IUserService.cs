using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;

namespace BookMeMobi2.Services
{
    public interface IUserService
    {
        Task<UserLoginDto> SignIn(Credentials credentials);
        Task<UserLoginDto> Register(UserRegisterDto userDto);
        Task Logout();
        PagedList<UserDto> GetAllUsers(int pageSize, int pageNumber);
        Task<User> GetUser(string userId);
    }
}
