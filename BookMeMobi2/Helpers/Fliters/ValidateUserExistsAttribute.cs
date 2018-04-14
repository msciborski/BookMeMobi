using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Entities;
using BookMeMobi2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BookMeMobi2.Helpers.Fliters
{
    public class ValidateUserExistsAttribute : TypeFilterAttribute
    {
        public ValidateUserExistsAttribute() : base(typeof(ValidateUserExistsFilterImpl)) { }

        private class ValidateUserExistsFilterImpl : IAsyncActionFilter
        {
            private readonly ApplicationDbContext _context;

            public ValidateUserExistsFilterImpl(ApplicationDbContext context)
            {
                _context = context;
            }
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.ActionArguments.ContainsKey("userId"))
                {
                    var userId = context.ActionArguments["userId"] as string;

                    if (!String.IsNullOrEmpty(userId))
                    {
                        if (await _context.Users.AllAsync(u => u.Id != userId))
                        {
                            context.Result = new JsonResult(new ApiError($"User with id {userId} dosen't exist")) {StatusCode = 404};
                            return;
                        }
                    }
                }

                await next();
            }
        }
    }
}
