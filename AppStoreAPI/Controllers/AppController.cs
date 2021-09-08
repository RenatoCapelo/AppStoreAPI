using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        [HttpGet]
        public string get()
        {
            return "Ok";
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> PostAplication([FromForm]string devGuid,[FromForm]IFormFile apkFile)
        {
            return Ok(new { devGuid, apkFile.ContentType,apkFile.FileName, Request.ContentLength});
        }
    }
}
