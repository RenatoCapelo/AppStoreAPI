using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using Dapper;
using AppStoreAPI.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public UserController(IConfiguration config, IWebHostEnvironment environment)
        {
            this.config = config;
            this.environment = environment;
        }
        // GET api/<UserController>/5
        [HttpGet("{uID}")]
        public IActionResult Get(string uID)
        {
            try
            {
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    var res = conn.QueryFirst<UserToGet>("Select guid,email,photo_guid,name,dob from Users where guid=@uID", new { uID });
                    res.Gender = conn.QueryFirst<UserGender>("Select Gender.* from Gender join Users on Gender.Id=Users.idGender where Users.guid=@uID;", new { uID });
                    return Ok(res);
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // POST api/<UserController>
        [HttpPost]
        public IActionResult Post([FromBody] UserToCreate user)
        {
            try
            {
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    var emailexists = conn.ExecuteScalar<int>("Select count(id) from Users where email=@email", new { user.email });
                    if (emailexists > 0)
                        return BadRequest("The user already exists");

                    Guid guid;
                    int guidExists;
                    do
                    {
                        guid = Guid.NewGuid();
                        guidExists = conn.ExecuteScalar<int>("Select count(guid) from Users where guid=@guid",new { guid = guid.ToString().ToUpper() });
                    } while (guidExists > 0);

                    user.password = Security.SHA512(user.password);
                    var gender = conn.QueryFirst<UserGender>("Select * from Gender where id=@id", new { id = user.idGender });
                    if (gender == null)
                        return BadRequest("Gender not found");

                    var insert = conn.ExecuteScalar<int>("Insert into Users(email,name,dob,idGender,password) values(@email,@name,@dob,@idGender,@password)", user);

                    return Ok(
                        new
                        {
                            guid,
                            user.email,
                            user.name,
                            user.dob,
                            gender
                        }
                        );
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        //// PUT api/<UserController>/5
        //[HttpPut("{id}/ProfilePicture")]
        //public async Task<IActionResult> PutAsync([FromForm] IFormFile file)
        //{
        //    return NotFound();
        //}

        [HttpPatch("{id}")]
        public void Patch(int id)
        {
            
        }
        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
