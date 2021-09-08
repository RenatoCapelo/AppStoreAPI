using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace AppStoreAPI.Controllers
{
    [ApiController]
    [Route("api/User/{guid}/ProfilePicture/")]
    public class ProfilePictureController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public ProfilePictureController(IConfiguration config, IWebHostEnvironment environment)
        {
            this.config = config;
            this.environment = environment;
        }


        [HttpGet]
        public async Task<IActionResult> GetAsync(string guid)
        {
            try
            {
                string namefile;
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    var userExists = "SELECT 0+count(*) FROM USERS WHERE guid=@guid";
                    if (conn.ExecuteScalar<int>(userExists, new { guid }) > 0)
                    {
                        var getpfp = "Select photoguid from users where guid = @guid";
                        var pfp = await conn.ExecuteScalarAsync(getpfp,new { guid});
                        if(pfp == null)
                        {
                            return NotFound();
                        }
                        namefile= Directory.EnumerateFiles($"{environment.ContentRootPath}/wwwroot/ProfilePictures/{guid}/{pfp}/").First();
                        namefile = Path.GetFileName(namefile);
                        return Redirect($"https://{Request.Headers["Host"]}/ProfilePictures/{guid}/{pfp}/{namefile}");
                    }
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(string guid, [FromForm]IFormFile file)
        {
            try
            {
                using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    var userExists = "SELECT 0+count(*) FROM USERS WHERE guid=@guid";
                    if (connection.ExecuteScalar<int>(userExists, new { guid }) == 0)
                        return NotFound();
                }

                var fileExtension = System.IO.Path.GetExtension(file.FileName);

                var ValidExtensions = new List<string>()
                {
                    ".bmp",
                    ".jpg",
                    ".jpeg",
                    ".gif",
                    ".png",
                    ".webp"
                };

                if (file.Length > 0 && ValidExtensions.Contains(fileExtension))
                {
                    try
                    {
                        var photoguid = Guid.NewGuid().ToString();
                        //Deletes Old Profile Picture in case it exists
                        if (Directory.Exists(environment.ContentRootPath + $"/wwwroot/ProfilePictures/{guid}/"))
                        {
                            Directory.Delete(environment.ContentRootPath + $"/wwwroot/ProfilePictures/{guid}/",true);
                        }

                        Directory.CreateDirectory(environment.ContentRootPath + $"/wwwroot/ProfilePictures/{guid}/{photoguid}/");

                        var namefile = file.FileName;

                        using (FileStream fs = System.IO.File.Create(environment.ContentRootPath + $"/wwwroot/ProfilePictures/{guid}/{photoguid}/" + namefile))
                        {
                            await file.CopyToAsync(fs);
                            fs.Flush();
                        }

                        using (var connection=new SqlConnection(config.GetConnectionString("DefaultConnection")))
                        {
                            var sqlInsertPhoto = "Update Users set photoguid=@photoguid where guid=@guid";
                            await connection.ExecuteAsync(sqlInsertPhoto,new {
                                guid,
                                photoguid
                            });
                        }
                        return StatusCode(201,$"https://{Request.Headers["Host"]}/ProfilePictures/{guid}/{photoguid}/" + namefile);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (NullReferenceException)
            {
                ModelState.AddModelError("File Error","The file uploaded was empty");
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteWithGuid(string guid)
        {
            try
            {
                if (Directory.Exists(environment.ContentRootPath + $"/wwwroot/ProfilePictures/{guid}/"))
                {
                    Directory.Delete(environment.ContentRootPath + $"/wwwroot/ProfilePictures/{guid}/", true);
                }
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    var res = await conn.ExecuteAsync("Update users set photoguid=null where guid=@guid", new { guid });
                    if (res > 0)
                    {
                        return Ok();
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
