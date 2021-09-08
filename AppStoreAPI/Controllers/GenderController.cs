using AppStoreAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenderController : ControllerBase
    {
        private readonly IConfiguration config;

        public GenderController(IConfiguration configuration)
        {
            this.config = configuration;
        }
        // GET: api/<GenderController>
        [HttpGet]
        public IActionResult Get()
        {
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                string sql = "Select * from Gender";
                return Ok(con.Query(sql));
            }
        }

        // GET api/<GenderController>/5
        [HttpGet("{id}",Name ="GetGenderById")]
        public IActionResult Get(int id)
        {
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                string sql = "Select * from Gender where id=@id";
                var res = con.Query(sql, new { id });
                if(res.Any())
                    return Ok(res.First());
                return NotFound();
            }
        }

        // POST api/<GenderController>
        [HttpPost]
        public IActionResult Post([FromBody] GenderToCreate gender)
        {
            try
            {
                using (var conn= new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    string sql = "Insert into Gender output Inserted.Id values(@description)";
                    var id = conn.ExecuteScalar<int>(sql,new { gender.description });
                    return CreatedAtRoute("GetGenderById", new { id },new {id,gender.description});
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    return BadRequest("There is already a gender with the same description!");
                else
                    return BadRequest();
            }
        }

        // PUT api/<GenderController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] GenderToCreate gender)
        {
            try
            {
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    string sqlExists = "Select count(*) from Gender where id=@id";
                    if(conn.ExecuteScalar<int>(sqlExists,new { id })<1)
                    {
                        return NotFound();
                    }
                    string sqlUpdate = "Update Gender set description=@description where id=@id";
                    var res = conn.Execute(sqlUpdate, new { gender.description, id });
                    return Ok(new { id, gender.description });
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    return BadRequest("There is already a gender with the same description!");
                else
                    return BadRequest();
            }
        }

        // DELETE api/<GenderController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                using (var conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                {
                    string sqlExists = "Select count(*) from Gender where id=@id";
                    if (conn.ExecuteScalar<int>(sqlExists, new { id }) < 1)
                    {
                        return NotFound();
                    }
                    string sqlUpdate = "Delete from Gender where id=@id";
                    var res = conn.Execute(sqlUpdate, new { id });
                    return Ok();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    return BadRequest("There is already a gender with the same description!");
                else
                    return BadRequest();
            }
        }
    }
}
