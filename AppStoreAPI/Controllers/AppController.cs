using ApkNet.ApkReader;
using AppStoreAPI.Models;
using Dapper;
using Dapper.Contrib.Extensions;
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
using System.Transactions;

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
        [HttpGet("{appGuid}", Name ="AppGet")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByGuid(Guid appGuid) 
        {
            using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                var sql = "Select Application.*,dbo.getRatingAverage(Application.id) as ratingAverage,ApplicationCategory.*,ApplicationMasterCategory.*,Developer.* from Application join ApplicationCategory on Application.idAppCategory=ApplicationCategory.id join ApplicationMasterCategory on ApplicationMasterCategory.id = ApplicationCategory.MasterCategoryID join Developer on Developer.id = Application.idDeveloper where application.applicationGuid=@guid";
                var res = connection.Query<Application_DBO, decimal?, ApplicationCategory, ApplicationMasterCategory, DeveloperToGet, AppToGet>(sql, (app, rating, category, master, dev) =>
                {
                    category.masterCategory = master;
                    return new AppToGet()
                    {
                        applicationGuid = app.applicationGuid,
                        applicationSize = app.applicationSize,
                        description = app.description,
                        developer = dev,
                        applicationCategory = category,
                        minSdkVersion = app.minsdkversion,
                        packageName = app.packageName,
                        title = app.title,
                        versionCode = app.versionCode,
                        versionName = app.versionName,
                        ratingAverage = rating.HasValue ? (double)rating.Value : 0,
                        dateOfPublish = app.dateOfPublish,
                        dateOfUpdate = app.dateOfUpdate
                    };
                },
                splitOn: "id,ratingAverage,id,id,id"
                , param: new { appGuid }).FirstOrDefault();
                if (res == null)
                    return NotFound();
                else
                    return Ok(res);
            }
        }
        
        /// <summary>
        /// Method that Allows to search by apps by multiple parameters
        /// </summary>
        /// <param name="sortBy">Accepts: Date or Downloads</param>
        /// <param name="orderBy">Accepts: ASC or DESC</param>
        /// <param name="devGuid">Accepts: A Dev's unique identifier</param>
        /// <param name="category">Accepts: A Category ID</param>
        /// <param name="masterCategory">Accepts: 1 for Apps or 2 for Games</param>
        /// <param name="pageNumber">Accepts: an integer that refeers to the requested page</param>
        /// <param name="pageSize">Accepts: an integer that refers to the count of elements to retrieve</param>
        /// <returns>A List of Apps that fulfill the requirements</returns>
        [AllowAnonymous]
        public IActionResult Search([FromQuery(Name = "sortBy")] string sortBy, [FromQuery(Name = "orderBy")] string orderBy, [FromQuery(Name ="devGuid")] string devGuid,[FromQuery(Name ="Category")] int? category, [FromQuery(Name = "MasterCategory")] int? masterCategory,[FromQuery(Name ="pageNumber")] int pageNumber=1,[FromQuery(Name = "pageSize")] int pageSize=5)
        {
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                int idDev=0;
                string sql = "Select Application.*,dbo.getRatingAverage(Application.id) as ratingAverage,dbo.getIconPhoto(Application.id) as icon,ApplicationCategory.*,ApplicationMasterCategory.*,Developer.* from Application join ApplicationCategory on Application.idAppCategory=ApplicationCategory.id join ApplicationMasterCategory on ApplicationMasterCategory.id = ApplicationCategory.MasterCategoryID join Developer on Developer.id = Application.idDeveloper";
                sql += " where";
                if (!string.IsNullOrEmpty(devGuid)) {
                    idDev = con.ExecuteScalar<int>("Select id from Developer where devGuid=@devGuid", new {devGuid});
                    sql += " Application.idDeveloper=@idDev and"; 
                }
                if (category.HasValue) { sql += " Application.idAppCategory=@category and"; }
                else
                    category = 0;
                if (masterCategory.HasValue) { sql += " ApplicationCategory.MasterCategoryID=@masterCategory and"; }
                else
                    masterCategory = 0;
                sql += " 1=1";
                if(!string.IsNullOrEmpty(sortBy)) 
                {
                    bool sortValid = false;
                    switch (sortBy.ToLowerInvariant())
                    {
                        case "date":
                            sortValid = true;
                            sql += " order by Application.id";
                            break;
                        case "downloads":
                            sortValid = true;
                            sql += " order by dbo.getDownloadCount(Application.id)";
                            break;
                        default:
                            break;
                    }

                    if (!string.IsNullOrEmpty(orderBy) && sortValid)
                    {
                        switch (orderBy.ToLowerInvariant())
                        {
                            case "asc":
                                sql += " ASC";
                                break;
                            case "desc":
                                sql += " DESC";
                                break;
                            default:
                                break;
                        }
                    }
                }

                var results = con.Query<Application_DBO,decimal?,Guid?,ApplicationCategory,ApplicationMasterCategory,DeveloperToGet,AppToGet>(sql, (app,rating,icon,category,master,dev) => 
                {
                    category.masterCategory = master;
                    return new AppToGet()
                    {
                        applicationGuid = app.applicationGuid,
                        applicationSize = app.applicationSize,
                        developer = dev,
                        applicationCategory = category,
                        description = app.description,
                        minSdkVersion = app.minsdkversion,
                        packageName = app.packageName,
                        title = app.title,
                        versionCode = app.versionCode,
                        versionName = app.versionName,
                        ratingAverage = rating.HasValue ? (double)rating.Value : 0,
                        dateOfPublish = app.dateOfPublish,
                        dateOfUpdate = app.dateOfUpdate,
                        Icon = icon
                    };
                },
                splitOn:"id,ratingAverage,icon,id,id,id"
                ,param:new {idDev,category=category.Value,masterCategory=masterCategory.Value});

                return Ok(new
                {
                    pages = (int)Math.Ceiling(results.Count()/(double)pageSize),
                    currentPage = pageNumber,
                    count = results.Count(),
                    results = results.Skip((pageNumber-1) * pageSize).Take(pageSize)
                });
            }
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> PublishApplication([FromForm]ApplicationToPost appToPublish)
        {
            var guid = Guid.Parse(User.FindFirst("Guid").Value);
            Developer_DBO dev;

            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                string sql = "Select Developer.* from Developer join Users on Users.id = Developer.idUser where Users.guid = @userGuid;";
                con.Open();
                dev = await con.QueryFirstAsync<Developer_DBO>(sql, new {userGuid = guid});
                if (dev is null)
                {
                    ModelState.AddModelError("devGuid", "The guid provided doesn't refer to any developer registered");
                }
                var appCategory = await con.GetAsync<ApplicationCategory>(appToPublish.idAppCategory);
                if (appCategory is null)
                {
                    ModelState.AddModelError("ApplicationCategory","The id provided doesn't match with any category");
                }
            
                var fileExtension = System.IO.Path.GetExtension(appToPublish.apk.FileName);
                var validExtensions = new List<string>()
                {
                    ".apk"
                };
                if (!validExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("apkFile", "The File is not in an APK Format or its empty");
                }
                if (ModelState.IsValid)
                {
                    byte[] manifestData = null;
                    byte[] resourcesData = null;

                    var appGuid = Guid.NewGuid();
                    using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        while (conn.ExecuteScalar<int>("Select count(*)+0 from Application where ApplicationGuid=@appGuid",new { appGuid }) !=0)
                        {
                            appGuid = Guid.NewGuid();
                        }
                    }
                    Directory.CreateDirectory($"{ environment.ContentRootPath}/temp/apps/{ appGuid}/");

                    try
                    {
                        AppInfo appInfo;
                        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            using (var fileStream = System.IO.File.Create($"{ environment.ContentRootPath}/temp/apps/{appGuid}/" + appToPublish.apk.FileName))
                            {
                                await appToPublish.apk.CopyToAsync(fileStream);
                            }

                            var path = $"{environment.ContentRootPath}/temp/apps/{appGuid}/" + appToPublish.apk.FileName;
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

                            appInfo = new AppInfo()
                            {
                                appGuid = appGuid,
                                devGuid = dev.devGuid,
                                packageName = info.packageName,
                                versionName = info.versionName,
                                label = info.label,
                                versionCode = info.versionCode,
                                minSdkVersion = info.minSdkVersion,
                                targetSdkVersion = info.targetSdkVersion,
                                Permissions = info.Permissions,

                                supportAnyDensity = info.supportAnyDensity,
                                supportLargeScreens = info.supportLargeScreens,
                                supportNormalScreens = info.supportNormalScreens,
                                supportSmallScreens = info.supportSmallScreens
                            };

                            Application_DBO application_DBO = new Application_DBO()
                            {
                                applicationGuid = appGuid,
                                applicationSize = appToPublish.apk.Length,
                                idAppCategory = appToPublish.idAppCategory,
                                idDeveloper = dev.id,
                                minsdkversion = int.Parse(appInfo.minSdkVersion),
                                packageName = info.packageName,
                                title = appToPublish.title,
                                versionCode = int.Parse(appInfo.versionCode),
                                versionName = appInfo.versionName,
                                description=appToPublish.description,
                                dateOfPublish =DateTime.Now,
                                dateOfUpdate = DateTime.Now,
                            };

                            con.Insert(application_DBO);

                            Directory.CreateDirectory($"{environment.ContentRootPath}/wwwroot/apps/{dev.devGuid}/{ appGuid}/");
                            using (FileStream fs_1 = System.IO.File.Create($"{ environment.ContentRootPath}/wwwroot/apps/{dev.devGuid}/{appGuid}/{info.label}.apk"))
                            {
                                await appToPublish.apk.CopyToAsync(fs_1);
                            }
                            AppToGet appToGet = new AppToGet()
                            {
                                applicationCategory = appCategory,
                                description = application_DBO.description,
                                applicationGuid = appGuid,
                                applicationSize = application_DBO.applicationSize,
                                developer = new DeveloperToGet() { createdOn=dev.createdOn,devGuid=dev.devGuid, devName=dev.devName,phoneNum=dev.phoneNum,secEmail=dev.secEmail},
                                minSdkVersion = application_DBO.minsdkversion,
                                packageName=application_DBO.packageName,
                                title=application_DBO.title,
                                versionCode=application_DBO.versionCode,
                                versionName=application_DBO.versionName,
                                dateOfPublish = application_DBO.dateOfPublish,
                                dateOfUpdate = application_DBO.dateOfUpdate,
                            };
                            transactionScope.Complete();
                            return CreatedAtRoute("AppGet",  routeValues: new {appGuid = appToGet.applicationGuid} , value: appToGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Directory.Exists($"{ environment.ContentRootPath}/wwwroot/apps/{dev.devGuid}/{ appGuid}/"))
                            Directory.Delete($"{ environment.ContentRootPath}/wwwroot/apps/{dev.devGuid}/{ appGuid}/", true);
                        return BadRequest(ex.Message);
                    }
                    finally
                    {
                        con.Close();
                        if (Directory.Exists($"{ environment.ContentRootPath}/temp/apps/{appGuid}/"))
                        {
                            Directory.Delete($"{ environment.ContentRootPath}/temp/apps/{appGuid}/", true);
                        }
                    }
                }
                else
                {
                    con.Close();
                    return BadRequest(ModelState);
                }
            }
        }

        [HttpPut]
        [Authorize(Roles = "Dev")]
        public async Task<IActionResult> UpdateAplication([FromForm]ApplicationToPost appToUpdate)
        {
            return StatusCode(405);
        }

        [AllowAnonymous]
        [HttpGet("Photos/{appGuid}")]
        public async Task<IActionResult> GetAppPhotos(Guid appGuid)
        {
            return Ok(new { appGuid });
        }
    }
}
