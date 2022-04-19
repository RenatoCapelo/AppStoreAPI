using AppStoreAPI.Models;
using AppStoreAPI.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
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
    [Route("[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly IConfiguration config;

        public DeveloperController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpPost]
        [Authorize(Roles="User")]
        public async Task<IActionResult> PostDeveloper([FromBody]DeveloperToCreate dev)
        {
            try {
            /*
             var claims = User.Claims.ToList();
             var userGuid = claims?.FirstOrDefault(x => x.Type.Equals("Guid", StringComparison.OrdinalIgnoreCase))?.Value;
             */
            var userGuid = User.Claims.ToList().FirstOrDefault(x => x.Type.Equals("Guid", StringComparison.OrdinalIgnoreCase))?.Value;
            Developer_DBO developer = new Developer_DBO();
            developer.devName = dev.devName;
            developer.secEmail = String.IsNullOrWhiteSpace(dev.secEmail) ? null : dev.secEmail;
            developer.phoneNum = dev.phoneNum;
            

            using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                string sql = "Select id from Users where guid=@guid";
                var id = await conn.ExecuteScalarAsync<int?>(sql, new { guid = userGuid });
                if (!id.HasValue)
                {
                    ModelState.AddModelError("User","No User was found with the provided credentials");
                    //return NotFound(ModelState.Values.Select(e => e.Errors).ToList());
                    return NotFound(ModelState);
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
                    ModelState.AddModelError("Developer", "There is already a developer registered with this Name");
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

                string sqlUpdateUser = "Update Users set idRole=2 where id=@id";
                var res = conn.Execute(sqlUpdateUser,new { id });

                string sqlGetUser = "Select * from Users where guid=@guid"; //"Select guid,email,photoguid,name,dob,idGender from Users where email=@email and password=@password";
                var result = conn.QueryFirstOrDefault<User_DBO>(sqlGetUser, new { guid=userGuid });
                if (result == null)
                    return NotFound();
                UserToGet userToGet = new UserToGet()
                {
                    Dob = result.dob,
                    Email = result.email,
                    PhotoGuid = result.photoguid,
                    Name = result.name,
                    Guid = result.guid,
                };
                userToGet.Gender = conn.QueryFirstOrDefault<UserGender>("Select * from Gender where id=@idGender", new { result.idGender });
                userToGet.Developer = conn.QueryFirstOrDefault<DeveloperToGet>("Select Developer.*, Users.guid as 'userGuid' from Developer join Users on Developer.idUser = Users.id where idUser = @id", new { result.id });
                userToGet.Role = conn.QueryFirstOrDefault<string>("Select [description] from UserRoles where id=@idRole", new { result.idRole });
                return Ok(new { user=userToGet,token= TokenService.GenerateToken(userToGet) });
            }
            }
            catch (Exception ex)
            {
                return StatusCode(200, ex.Message);
            }
        }
    }
}
