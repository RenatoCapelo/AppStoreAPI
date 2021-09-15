using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppStoreAPI.Models;
using Microsoft.IdentityModel.Tokens;
using JWT.Algorithms;
using JWT.Builder;

namespace AppStoreAPI.Services
{
    public static class TokenService
    {
        public static string GenerateToken(UserToGet user)
        {
            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .ExpirationTime(DateTime.Now.AddDays(1))
                .WithSecret(Security.key)
                .AddClaim("Guid",user.guid)
                .AddClaim("Name", user.name)
                .AddClaim("Role", user.roleId)
                .Encode();
        }
    }
}
