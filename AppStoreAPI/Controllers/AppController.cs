using ApkNet.ApkReader;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration config;

        public AppController(IWebHostEnvironment environment, IConfiguration config)
        {
            this.environment = environment;
            this.config = config;
        }
        [HttpGet]
        public string get()
        {
            return "Ok";
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> PostAplication([FromForm]string devGuid,[FromForm]IFormFile file)
        {

            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                string sql = "Select 0+count(*) from Developer where devGuid=@devGuid";
                if (await con.ExecuteScalarAsync<int>(sql, new { devGuid }) == 0)
                {
                    ModelState.AddModelError("devGuid", "The guid provided doesn't reffer to any developer registered");
                    return NotFound(ModelState.Values.Select(e => e.Errors).ToList());
                }
            }
            var fileExtension = System.IO.Path.GetExtension(file.FileName);
            var validExtensions = new List<string>()
            {
                ".apk"
            };
            if (file.Length > 0 && validExtensions.Contains(fileExtension))
            {
                byte[] manifestData = null;
                byte[] resourcesData = null;

                var appGuid = Guid.NewGuid().ToString();
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    while (conn.ExecuteScalar<int>("Select count(*)+0 from Application where AplicationGuid=@appGuid",new { appGuid }) !=0)
                    {
                        appGuid = Guid.NewGuid().ToString();
                    }
                }
                Directory.CreateDirectory($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/");

                try
                {
                    using (var fileStream = System.IO.File.Create($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/"+file.FileName))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var path = $"{ environment.ContentRootPath}/temp/apps/{ appGuid}/" + file.FileName;
                    var zipFile = ZipFile.OpenRead(path);
                    foreach (var item in zipFile.Entries)
                    {
                        if (item.Name.ToLower() == "androidmanifest.xml")
                        {
                            manifestData = new byte[50 * 1024];
                            using (Stream strm = item.Open())
                            {
                                strm.Read(manifestData, 0, manifestData.Length);
                            }

                        }
                        if (item.Name.ToLower() == "resources.arsc")
                        {

                            using (Stream strm = item.Open())
                            {
                                using (BinaryReader s = new BinaryReader(strm))
                                {
                                    resourcesData = s.ReadBytes((int)item.Length);
                                }
                            }
                        }
                    }
                    zipFile.Dispose();

                    ApkReader apkReader = new ApkReader();
                    ApkInfo info = apkReader.extractInfo(manifestData, resourcesData);
                    return Ok(info.versionName);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                finally
                {
                    if (Directory.Exists($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/"))
                    {
                        Directory.Delete($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/", true);
                    }
                }

                //Directory.CreateDirectory($"{ environment.ContentRootPath}/wwwroot/apps/{ devGuid}/{ appGuid}/");
            }
            else
            {
                ModelState.AddModelError("apkFile", "The File is not in an APK Format or its empty");
                return BadRequest(ModelState);
            }

        }
    }
}
