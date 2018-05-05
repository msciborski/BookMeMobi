using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookMeMobi2.Hangfire
{
    public class CustomAuthorizeFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContent = context.GetHttpContext();
            return httpContent.User.Identity.IsAuthenticated && httpContent.User.Identity.Name == "Anus12";
        }
    }
}
