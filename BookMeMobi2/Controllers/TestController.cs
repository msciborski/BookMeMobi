using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BookMeMobi2.Controllers
{
    [Route("api/test")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return new JsonResult(new { Id= "asdasda", Name = "Test, test" });
        }
    }
}