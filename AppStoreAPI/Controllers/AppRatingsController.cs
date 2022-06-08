using AppStoreAPI.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AppStoreAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AppRatingsController : ControllerBase
    {
        private readonly IConfiguration config;

        public AppRatingsController(IConfiguration config)
        {
            this.config = config;
        }
        [HttpGet("{guid}")]
        public async Task<IActionResult> getRatingsByApp([FromRoute] Guid guid)
        {
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                var ratings = await con.QueryAsync<ApplicationRatings_dbo,UserToGet,DeveloperToGet,ApplicationRating>("SELECT ApplicationRatings.*,Users.*,Developer.* FROM ApplicationRatings join Users on ApplicationRatings.idUser=Users.id left join Developer on Developer.idUser=Users.id join Application on ApplicationRatings.idApplication=Application.id where Application.applicationGuid=@guid", (rating, author, developer)=>{
                    author.Developer= developer;
                    return new ApplicationRating(){ 
                        Author = author,
                        Rating = rating.rating,
                        Comment=rating.comment,
                        Id=rating.id
                    };
                } ,new {guid});
                con.Close();
                return Ok(ratings);
            }
        }
        [HttpGet("{guid}/user")]
        [Authorize]
        public async Task<IActionResult> getRatingsByAppandUser([FromRoute] Guid guid)
        {
            var userGuid = Guid.Parse(User.FindFirst("Guid").Value);
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                var ratings = await con.QueryAsync<ApplicationRatings_dbo, UserToGet, DeveloperToGet, ApplicationRating>("SELECT ApplicationRatings.*,Users.*,Developer.* FROM ApplicationRatings join Users on ApplicationRatings.idUser=Users.id join Developer on Developer.idUser=Users.id join Application on ApplicationRatings.idApplication=Application.id where Application.applicationGuid=@guid and Users.guid=@userGuid",(rating, author, developer)=>{
                    author.Developer = developer;
                    return new ApplicationRating()
                    {
                        Author = author,
                        Rating = rating.rating,
                        Comment = rating.comment,
                        Id = rating.id
                    };
                } , new { guid,userGuid });
                con.Close();
                return Ok(ratings);
            }
        }

        [HttpPost("{guid}")]
        [Authorize]
        public async Task<IActionResult> postRating([FromRoute] Guid guid,[FromBody] ApplicationRating_ToPost comment)
        {
            var userGuid = Guid.Parse(User.FindFirst("Guid").Value);
           
            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                var idUser = await con.QueryFirstAsync<int>("SELECT id FROM Users where guid=@userGuid", new {userGuid});
                var idApp = await con.QueryFirstAsync<int>("Select id From Application where applicationGuid=@guid", new { guid });
                var rating = new ApplicationRatings_dbo()
                {  
                    idUser = idUser,
                    comment=comment.message,
                    rating = comment.rating,
                    idApplication = idApp
                };
                await con.InsertAsync(rating);
                con.Close();
            }
            return Ok();
        }

        [HttpPut("{guid}")]
        [Authorize]
        public async Task<IActionResult> updateRating([FromRoute] Guid guid, [FromBody] ApplicationRating_ToPost comment)
        {
            var userGuid = Guid.Parse(User.FindFirst("Guid").Value);

            using (var con = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                var idUser = await con.QueryFirstAsync<int>("SELECT id FROM Users where guid=@userGuid", new { userGuid });
                var idApp = await con.QueryFirstAsync<int>("Select id From Application where applicationGuid=@guid", new { guid });
                var idComment = await con.QueryFirstAsync<int>("Select id from ApplicationRatings where idUser=@idUser", new { idUser });
                var rating = new ApplicationRatings_dbo()
                {
                    id=idComment,
                    idUser = idUser,
                    comment = comment.message,
                    rating = comment.rating,
                    idApplication = idApp
                };
                await con.UpdateAsync(rating);
                con.Close();
            }
            return Ok();
        }
    }
}
