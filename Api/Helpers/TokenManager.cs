using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Api.Helpers
{
    /// <summary>
    /// Token manager
    /// </summary>
    public static class TokenManager
    {
        /// <summary>
        /// Generate token for a specific user
        /// </summary>
        /// <param name="secret">Secret key</param>
        /// <param name="userId">User id</param>
        public static string GenerateToken(string secret, int userId)
        {
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]  { new Claim(ClaimTypes.Name, userId.ToString(CultureInfo.InvariantCulture)) }),
                Expires = DateTime.Now.AddMonths(6),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
