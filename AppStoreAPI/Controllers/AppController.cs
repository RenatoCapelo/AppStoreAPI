using ApkNet.ApkReader;
using AppStoreAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
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
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppStoreAPI.Controllers
{
    [Route("[controller]")]
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
        [Authorize(Roles="Admin")]
        public IActionResult Get()
        {
            return Ok();
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> PublishApplication([FromForm]ApplicationToPost appToPublish)
        {
            var guid = Guid.Parse(User.FindFirst("Guid").Value);
            Guid? dev;

            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                string sql = "Select Developer.devGuid from Developer join Users on Users.id = Developer.idUser where Users.guid = @userGuid;";
                dev = await con.ExecuteScalarAsync<Guid?>(sql, new {userGuid = guid});
                if (!dev.HasValue)
                {
                    ModelState.AddModelError("devGuid", "The guid provided doesn't refer to any developer registered");
                    return NotFound(ModelState.Values.Select(e => e.Errors).ToList());
                }
            }
            var fileExtension = System.IO.Path.GetExtension(appToPublish.file.FileName);
            var validExtensions = new List<string>()
            {
                ".apk"
            };
            if (appToPublish.file.Length > 0 && validExtensions.Contains(fileExtension))
            {
                byte[] manifestData = null;
                byte[] resourcesData = null;

                var appGuid = Guid.NewGuid();
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    while (conn.ExecuteScalar<int>("Select count(*)+0 from Application where AplicationGuid=@appGuid",new { appGuid }) !=0)
                    {
                        appGuid = Guid.NewGuid();
                    }
                }
                Directory.CreateDirectory($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/");

                try
                {
                    using (var fileStream = System.IO.File.Create($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/"+ appToPublish.file.FileName))
                    {
                        await appToPublish.file.CopyToAsync(fileStream);
                    }

                    var path = $"{ environment.ContentRootPath}/temp/apps/{ appGuid}/" + appToPublish.file.FileName;
                    var zipFile = ZipFile.OpenRead(path);
                    foreach (var item in zipFile.Entries)
                    {
                        if (item.Name.ToLower() == "androidmanifest.xml")
                        {
                            manifestData = new byte[50 * 1024];
                            using (Stream strm = item.Open())
                            {
                                await strm.ReadAsync(manifestData, 0, manifestData.Length);
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

                    var appInfo = new AppInfo()
                    {
                        appGuid=appGuid,
                        devGuid= dev.Value,
                        packageName = info.packageName,
                        versionName = info.versionName,
                        label=info.label,
                        versionCode = info.versionCode,
                        hasIcon = info.hasIcon,
                        minSdkVersion = info.minSdkVersion,
                        targetSdkVersion=info.targetSdkVersion,
                        Permissions=info.Permissions,

                        supportAnyDensity=info.supportAnyDensity,
                        supportLargeScreens=info.supportLargeScreens,
                        supportNormalScreens=info.supportNormalScreens,
                        supportSmallScreens=info.supportSmallScreens
                    };
                    if (appInfo.hasIcon)
                    {
                        appInfo.iconFileName = info.iconFileName[0];
                    }
                    Directory.CreateDirectory($"{ environment.ContentRootPath}/wwwroot/apps/{dev}/{ appGuid}/");
                    using (FileStream fs_1 = System.IO.File.Create($"{ environment.ContentRootPath}/wwwroot/apps/{dev}/{appGuid}/{info.label}.apk"))
                    {
                        await appToPublish.file.CopyToAsync(fs_1);
                    }
                    return Ok(appInfo);
                }
                catch (Exception ex)
                {
                    if (Directory.Exists($"{ environment.ContentRootPath}/wwwroot/apps/{dev}/{ appGuid}/"))
                        Directory.Delete($"{ environment.ContentRootPath}/wwwroot/apps/{dev}/{ appGuid}/", true);
                    return BadRequest(ex.Message);
                }
                finally
                {
                    if (Directory.Exists($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/"))
                    {
                        Directory.Delete($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/", true);
                    }
                }

            }
            else
            {
                ModelState.AddModelError("apkFile", "The File is not in an APK Format or its empty");
                return BadRequest(ModelState);
            }

        }

        [HttpPut]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> UpdateAplication([FromForm]ApplicationToPost appToUpdate)
        {
            return StatusCode(405);
        }
    }
}
