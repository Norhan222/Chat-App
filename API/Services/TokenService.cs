using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IConfiguration config,UserManager<AppUser> userManager)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["tokenKey"]));
            _userManager = userManager;
        }
        public async  Task<string> CreateToken(AppUser user)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName),
            };
            var roles=await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role=>new Claim(ClaimTypes.Role,role)));

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature)
            };
            var TokenHandler = new JwtSecurityTokenHandler();
            var token=TokenHandler.CreateToken(tokenDescriptor);
            return TokenHandler.WriteToken(token);
        }
    }
}
