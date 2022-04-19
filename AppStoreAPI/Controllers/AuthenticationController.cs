using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using AppStoreAPI.Models;
using AppStoreAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace AppStoreAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] UserToAuth userToAuth)
        {
            //TODO - Refactor; add model to return object
            try
            {
                userToAuth.password = Security.SHA512(userToAuth.password);

                using (var connection = new SqlConnection(Strings.ConnectionString))
                {
                    string sqlAuth = "Select * from Users where email=@email and password=@password"; //"Select guid,email,photoguid,name,dob,idGender from Users where email=@email and password=@password";
                    var result = await connection.QueryFirstOrDefaultAsync<User_DBO>(sqlAuth, userToAuth);
                    if (result == null)
                        return NotFound();
                    UserToGet userToGet = new UserToGet()
                    {
                        Dob=result.dob,
                        Email=result.email,
                        PhotoGuid=result.photoguid,
                        Name=result.name,
                        Guid=result.guid,
                    };
                    connection.Open();
                    var userGenderTask = connection.QueryFirstOrDefaultAsync<UserGender>("Select * from Gender where id=@idGender",new { result.idGender});
                    var userDeveloperTask = connection.QueryFirstOrDefaultAsync<DeveloperToGet>("Select Developer.*, Users.guid as 'userGuid' from Developer join Users on Developer.idUser = Users.id where idUser = @id",new { result.id });
                    var userRoleTask = connection.QueryFirstOrDefaultAsync<string>("Select [description] from UserRoles where id=@idRole",new {result.idRole});
                    Task.WaitAll(userDeveloperTask,userDeveloperTask,userRoleTask);
                    connection.Close();
                    userToGet.Gender = userGenderTask.Result;
                    userToGet.Developer = userDeveloperTask.Result;
                    userToGet.Role = userRoleTask.Result;
                    return Ok(new { user=userToGet, token=TokenService.GenerateToken(userToGet) });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateToken()
        {
            return Ok();
        }
    }
}
