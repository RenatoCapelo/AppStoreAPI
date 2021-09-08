using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Hosting;

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        

        public DevController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }
        [HttpGet]
        public IActionResult GetUrl()
        {
            return Ok(Request.Headers);
        }
    }
}
