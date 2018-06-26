using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookMeMobi2.Helpers.Fliters
{
    public class ValidateTokenAttribute : TypeFilterAttribute
    {
        public ValidateTokenAttribute() : base(typeof(ValidateTokenFilterImpl))
        {
        }

        public class ValidateTokenFilterImpl : IAsyncActionFilter
        {
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.ActionArguments.ContainsKey("userId"))
                {
                    var userId = context.ActionArguments["userId"] as string;
                    var userIdFromToken = context.HttpContext.User.Identity.Name;
                    if (!userId.Equals(userIdFromToken))
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
                await next();
            }
        }
    }
}