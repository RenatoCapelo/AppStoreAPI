using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using AppStoreAPI.Models;

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        [HttpPost]
        public IActionResult AuthenticateUser([FromBody] UserToAuth userToAuth)
        {
            try
            {
                userToAuth.password = Security.SHA512(userToAuth.password);

                using (var connection = new SqlConnection(Strings.connectionString))
                {
                    string sqlAuth = "Select guid,email,photoguid,name,dob,idGender from Users where email=@email and password=@password";
                    var result = connection.Query(sqlAuth, userToAuth);
                    if (!result.Any())
                        return NotFound();
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }        
    }
}
