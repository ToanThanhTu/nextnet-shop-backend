using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using net_backend.Data.Types;

namespace net_backend.Users
{
    public class JwtTokenHelper
    {
        public static string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("my_super_secret_key_1234567890abcdef"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var userToken = new JwtSecurityToken(
                issuer: "Trevor",
                audience: "Trevor",
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(userToken);
        }
    }
}
