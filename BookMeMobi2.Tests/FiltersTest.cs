using System;
using System.Collections.Generic;
using System.Text;
using BookMeMobi2.Helpers.Fliters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace BookMeMobi2.Tests
{
    public class FiltersTest
    {
        [Fact]
        public void InvalidModelState_ShouldReturn_JsonResult_StatusCode_400()
        {
            var loggerMock = new Mock<ILogger<ValidateModelAttribute>>();
            var httpContext = new DefaultHttpContext();
            var context = new ActionExecutingContext(
                new ActionContext()
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>().Object);

            context.ModelState.AddModelError("testError","errorErrror");
            var sut = new ValidateModelAttributeImpl(loggerMock.Object);
            
            sut.OnActionExecuting(context);

            context.Result.ShouldNotBeNull();
        }
    }
}
