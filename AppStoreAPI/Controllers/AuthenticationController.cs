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

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        [HttpPost]
        public IActionResult AuthenticateUser([FromBody] UserToAuth userToAuth)
        {
            //TODO - Refactor; add model to return object
            try
            {
                userToAuth.password = Security.SHA512(userToAuth.password);

                using (var connection = new SqlConnection(Strings.connectionString))
                {
                    string sqlAuth = "Select * from Users where email=@email and password=@password"; //"Select guid,email,photoguid,name,dob,idGender from Users where email=@email and password=@password";
                    var result = connection.QueryFirst<User_DBO>(sqlAuth, userToAuth);
                    if (result == null)
                        return NotFound();
                    UserToGet userToGet = new UserToGet()
                    {
                        dob=result.dob,
                        email=result.email,
                        photo_guid=result.photoguid,
                        name=result.name,
                        guid=result.guid
                    };
                    userToGet.Gender = connection.QueryFirstOrDefault<UserGender>("Select * from Gender where id=@idGender",new { result.idGender});
                    userToGet.developer = connection.QueryFirstOrDefault<DeveloperToGet>("Select Developer.*, Users.guid as 'userGuid' from Developer join Users on Developer.idUser = Users.id where idUser = @id",new { result.id });

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
