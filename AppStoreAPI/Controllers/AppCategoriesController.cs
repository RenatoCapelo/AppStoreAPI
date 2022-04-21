using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;
using Dapper;
using System.Threading;
using AppStoreAPI.Models;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppStoreAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize()]
    public class AppCategoriesController : ControllerBase
    {
        IConfiguration config;
        AppCategoriesController(IConfiguration configuration)
        {
            this.config = configuration;
        }
        // GET: api/<AppCategoriesController>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAsync()
        {
            using (var connection=new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var results = await connection.QueryAsync<ApplicationCategory,ApplicationMasterCategory,ApplicationCategory>("Select ApplicationCategory.id, ApplicationCategory.name, ApplicationMasterCategory.id, ApplicationMasterCategory.name from ApplicationCategory join ApplicationMasterCategory on ApplicationCategory.MasterCategoryID=ApplicationMasterCategory.id order by ApplicationMasterCategory.id, ApplicationCategory.id", (category, master) =>
                {
                    category.masterCategory = master;
                    return category;
                });
                connection.Close();
                return Ok(results);
            }
        }

        // GET api/<AppCategoriesController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var results = await connection.QueryAsync<ApplicationCategory, ApplicationMasterCategory, ApplicationCategory>("Select ApplicationCategory.id, ApplicationCategory.name, ApplicationMasterCategory.id, ApplicationMasterCategory.name from ApplicationCategory join ApplicationMasterCategory on ApplicationCategory.MasterCategoryID=ApplicationMasterCategory.id where ApplicationCategory.id=@id order by ApplicationMasterCategory.id, ApplicationCategory.id", (category, master) =>
                {
                    category.masterCategory = master;
                    return category;
                }, param: new {id});
                connection.Close();
                return Ok(results.First());
            }
        }

        [HttpGet("GetByMaster/{id}")]
        public async Task<IActionResult> GetByMasterCategoryAsync(int id)
        {
            using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var results = await connection.QueryAsync<ApplicationCategory, ApplicationMasterCategory, ApplicationCategory>("Select ApplicationCategory.id, ApplicationCategory.name, ApplicationMasterCategory.id, ApplicationMasterCategory.name from ApplicationCategory join ApplicationMasterCategory on ApplicationCategory.MasterCategoryID=ApplicationMasterCategory.id where ApplicationMasterCategory.id=@id order by ApplicationMasterCategory.id, ApplicationCategory.id", (category, master) =>
                {
                    category.masterCategory = master;
                    return category;
                }, param: new { id });
                connection.Close();
                return Ok(results);
            }
        }

        // POST api/<AppCategoriesController>
        [HttpPost]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Post([FromBody]ApplicationCategoryToPost applicationCategoryToPost)
        {
            using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"))) {
                var category = new ApplicationCategory();
                category.name = applicationCategoryToPost.name;
                connection.Open();
                category.masterCategory = await connection.QueryFirstAsync<ApplicationMasterCategory>("select * from ApplicationMasterCategory where id=@id", new {id=applicationCategoryToPost.masterCategoryID});
                if (category.masterCategory is null)
                {
                    ModelState.AddModelError("MasterCategory", "The id provided doesn't correspond to any result");
                    return BadRequest(ModelState);
                }
                int newCategoryID = await connection.ExecuteScalarAsync<int>("INSERT INTO ApplicationCategory(MasterCategoryID,name) OUTPUT INSERTED.id values(@masterCategoryID,@name)", applicationCategoryToPost);
                return Ok(newCategoryID);
                
            }
        }

        // PUT api/<AppCategoriesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AppCategoriesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
