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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> SignIn([FromBody] Credentials credentials)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userLoginDto = await _userService.SignIn(credentials);
                    return Ok(userLoginDto);
                }
                catch (AppException e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e.Message);
                    return BadRequest(e.Message);
                }
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userLoginDto = await _userService.Register(userDto);

                    return Ok(userLoginDto);
                }
                catch (AppException e)
                {
                    _logger.LogCritical($"Unable to register: {e.Message}");

                    return BadRequest(e.Message);
                }
            }
            _logger.LogCritical("Unexpected error occured.");

            return BadRequest("Unexpected error occured.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.Logout();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAllUsers([FromQuery(Name = "page_size" )]int pageSize = 10, [FromQuery(Name = "page_number")] int pageNumber = 1)
        {
            return Ok(_mapper.Map<PagedList<User>, PagedList<UserDto>>(_userService.GetAllUsers(pageSize, pageNumber)));
        }

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _userService.GetUser(userId);
                return Ok(_mapper.Map<User, UserDto>(user));
            }
            catch (UserNoFoundException e)
            {
                _logger.LogError(e.Message);
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"{e.Message}, {e.StackTrace}");
                return new JsonResult("Something went wrong on server. Sorry.") {StatusCode = 500};
            }
        }
    }
}
