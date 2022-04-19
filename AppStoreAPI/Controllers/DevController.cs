using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AppStoreAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    
    public class DevController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        

        public DevController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }
        [HttpPost]
        [Authorize]
        public IActionResult GetGuidFromToken()
        {
            return Ok(User.FindFirst("Guid").Value);
        }
    }
}
