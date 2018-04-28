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
using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Models;
using BookMeMobi2.Models.User;
using BookMeMobi2.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookMeMobi2.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public UsersController(IUserService userService, ILogger<UsersController> logger, IMapper mapper)
        {
            _mapper = mapper;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Login user with credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserLoginDto), 200)]
        [ProducesResponseType(typeof(string), 500)]
        [AllowAnonymous]
        [ValidateModel]
        [HttpPost("login")]
        public async Task<IActionResult> SignIn([FromBody] Credentials credentials)
        {
            var userLoginDto = await _userService.SignIn(credentials);
            return Ok(userLoginDto);
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserLoginDto), 201)]
        [ProducesResponseType(typeof(ApiError), 500)]
        [AllowAnonymous]
        [ValidateModel]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            var userLoginDto = await _userService.Register(userDto);

            return new JsonResult(userLoginDto) { StatusCode = 201 };
        }
        /// <summary>
        /// Logout user.
        /// </summary>
        /// <returns></returns>
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.Logout();
            return Ok();
        }

        /// <summary>
        /// Returns all users.
        /// </summary>
        /// <param name="pageSize">Page size, default = 10</param>
        /// <param name="pageNumber">Page number, default = 1</param>
        /// <returns></returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedList<UserDto>), 200)]
        [HttpGet]
        public IActionResult GetAllUsers([FromQuery(Name = "page_size")]int pageSize = 10, [FromQuery(Name = "page_number")] int pageNumber = 1)
        {
            return Ok(_userService.GetAllUsers(pageSize, pageNumber));
        }

        /// <summary>
        /// Returns particular user.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns></returns>
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [AllowAnonymous]
        [ValidateUserExists]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await _userService.GetUser(userId);
            return Ok(_mapper.Map<User, UserDto>(user));
        }
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ValidateUserExists]
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UserUpdateDto model)
        {
            await _userService.UpdateUserAsync(userId, model);
            return NoContent();
        }
    }
}
