using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookMeMobi2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace BookMeMobi2.Helpers.Fliters
{
    public class ValidateModelAttribute : TypeFilterAttribute
    {
        public ValidateModelAttribute() : base(typeof(ValidateModelAttributeImpl))
        {
        }
    }
    public class ValidateModelAttributeImpl : IActionFilter
    {
        private readonly ILogger<ValidateModelAttribute> _logger;

        public ValidateModelAttributeImpl(ILogger<ValidateModelAttribute> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                _logger.LogError(GetErrorFromModelState(context.ModelState));
                context.Result = new JsonResult(new ApiError(context.ModelState)) {StatusCode = 400};
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private string GetErrorFromModelState(ModelStateDictionary modelStateDictionary)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, ModelStateEntry> pair in modelStateDictionary)
            {
                foreach (var error in pair.Value.Errors)
                {
                    builder.Append($"{pair.Key}:{error.ErrorMessage} ");
                }
            }

            return builder.ToString();
        }
    }
}
