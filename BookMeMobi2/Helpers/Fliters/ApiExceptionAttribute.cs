using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMeMobi2.Helpers.Exceptions;
using BookMeMobi2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace BookMeMobi2.Helpers.Fliters
{
    public class ApiExceptionAttribute : TypeFilterAttribute
    {
        public ApiExceptionAttribute() : base(typeof(ApiExceptionAttribute))
        {
        }
    }
    public class ApiExceptionAttributeImpl : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionAttributeImpl> _logger;

        public ApiExceptionAttributeImpl(ILogger<ApiExceptionAttributeImpl> logger)
        {
            _logger = logger;
        }
        public override void OnException(ExceptionContext context)
        {
            ApiError apiError = null;

            if (context.Exception is UserNoFoundException)
            {
                var ex = context.Exception as UserNoFoundException;
                context.Exception = null;
                apiError = new ApiError(ex.Message);

                _logger.LogError(ex.Message);

                context.HttpContext.Response.StatusCode = ex.StatusCode;
            }else if (context.Exception is BookNoFoundException)
            {
                var ex = context.Exception as BookNoFoundException;
                context.Exception = null;
                apiError = new ApiError(ex.Message);

                _logger.LogError(ex.Message);

                context.HttpContext.Response.StatusCode = ex.StatusCode;
            }else if (context.Exception is AppException)
            {
                var ex = context.Exception as AppException;
                context.Exception = null;
                apiError = new ApiError(ex.Message);

                _logger.LogError(ex.Message);

                context.HttpContext.Response.StatusCode = ex.StatusCode;
            }else if (context.Exception is UnauthorizedAccessException)
            {
                apiError = new ApiError("Unauthorized Access");

                _logger.LogError("Unauthorized access.");

                context.HttpContext.Response.StatusCode = 401;
            }
            else
            {
#if !DEBUG

                var msg = "An unhandled error occured.";
                string stack = null;
                var loggMsg = context.Exception.GetBaseException().Message;
                _logger.LogCritical(loggMsg);

#else
                var msg = context.Exception.GetBaseException().Message;
                string stack = context.Exception.StackTrace;
                _logger.LogCritical(msg);
#endif
                apiError = new ApiError(msg);
                apiError.Detail = stack;

                context.HttpContext.Response.StatusCode = 500;
            }

            context.Result = new JsonResult(apiError);
            base.OnException(context);
        }
    }
}
