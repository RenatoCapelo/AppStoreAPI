using AppStoreAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly IConfiguration config;

        public DeveloperController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpPost]
        public async Task<IActionResult> PostDeveloper([FromBody]DeveloperToCreate dev)
        {
            Developer_DBO developer = new Developer_DBO();
            developer.devName = dev.developerName;
            developer.secEmail = dev.secondEmail;
            developer.phoneNum = dev.phoneNum;

            using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                
                string sql = "Select id from Users where guid=@guid and password=@password";
                var id = await conn.ExecuteScalarAsync<int?>(sql, new { guid = dev.userGuid,password=Security.SHA512(dev.userPassword) });
                if (!id.HasValue)
                {
                    ModelState.AddModelError("User","No User was found with the provided credentials");
                    return NotFound(ModelState.Values.Select(e => e.Errors).ToList());
                }
                developer.idUser = id.Value;

                sql = "Select count(*) from developer where idUser=@idUser";
                if (conn.ExecuteScalar<int>(sql,new {developer.idUser}) > 0)
                {
                    ModelState.AddModelError("User","There is already a developer registered with this User");
                }
                sql = "Select count(*) from developer where devName=@devName";
                if (conn.ExecuteScalar<int>(sql, new { developer.devName }) > 0)
                {
                    ModelState.AddModelError("Name", "There is already a developer registered with this Name");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                developer.devGuid = Guid.NewGuid();
                while (conn.ExecuteScalar<int>("Select 0+count(*) from developer where devGuid=@devGuid",new {developer.devGuid }) != 0)
                {
                    developer.devGuid = Guid.NewGuid();
                }

                sql = "Insert into Developer values(@idUser,@devName,@secEmail,@phoneNum,@createdOn,@devGuid)";
                developer.createdOn = DateTime.Now;
                conn.Execute(sql,developer);
                return Ok(new
                {
                    developer.devGuid,
                    developer.devName,
                    developer.secEmail,
                    developer.phoneNum,
                    developer.createdOn
                });
            }
        }
    }
}
