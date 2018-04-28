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
    public class ValidateBookExistsAttribute : TypeFilterAttribute
    {
        public ValidateBookExistsAttribute() : base(typeof(ValidateBookExistsFilterImpl)) { }

        private class ValidateBookExistsFilterImpl : IAsyncActionFilter
        {
            private readonly ApplicationDbContext _context;

            public ValidateBookExistsFilterImpl(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.ActionArguments.ContainsKey("bookId"))
                {
                    var bookId = context.ActionArguments["bookId"] as int?;

                    if (bookId.HasValue)
                    {
                        if (await _context.Books.AllAsync(b => b.Id != bookId.Value))
                        {
                            context.Result = new JsonResult(new ApiError($"Book with id {bookId.Value} dosen't exist.")) {StatusCode = 404};
                            return;
                        }
                    }
                }

                await next();
            }
        }
    }
}
