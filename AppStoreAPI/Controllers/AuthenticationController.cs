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
        public IActionResult AuthenticateUser([FromBody] UserToAuth userToAuth)
        {
            //TODO - Refactor; add model to return object
            try
            {
                userToAuth.password = Security.SHA512(userToAuth.password);

                using (var connection = new SqlConnection(Strings.ConnectionString))
                {
                    string sqlAuth = "Select * from Users where email=@email and password=@password"; //"Select guid,email,photoguid,name,dob,idGender from Users where email=@email and password=@password";
                    var result = connection.QueryFirst<User_DBO>(sqlAuth, userToAuth);
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
                    userToGet.Gender = connection.QueryFirstOrDefault<UserGender>("Select * from Gender where id=@idGender",new { result.idGender});
                    userToGet.Developer = connection.QueryFirstOrDefault<DeveloperToGet>("Select Developer.*, Users.guid as 'userGuid' from Developer join Users on Developer.idUser = Users.id where idUser = @id",new { result.id });
                    userToGet.Role = connection.QueryFirstOrDefault<string>("Select [description] from UserRoles where id=@idRole",new {result.idRole});
                    return Ok(new { userToGet, token=TokenService.GenerateToken(userToGet) });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }        
    }
}
