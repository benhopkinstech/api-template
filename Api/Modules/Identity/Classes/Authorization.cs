using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;

namespace Api.Modules.Identity.Classes
{
    public class Authorization
    {
        public static string GenerateToken(IConfiguration config, Guid? accountId, string email)
        {
            string? tokenSecret = config.GetValue<string>("Jwt:TokenSecret");

            if (tokenSecret == null || accountId == null)
            {
                return "";
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim("sub", accountId.Value.ToString()),
                new Claim("email", email)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenSecret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config.GetValue<string>("Jwt:Issuer"),
                audience: config.GetValue<string>("Jwt:Audience"),
                claims: claims,
                notBefore: null,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public static Guid? GetAccountId(HttpContext http)
        {
            if (http.User.Claims == null)
                return null;

            var sub = http.User.Claims.FirstOrDefault(x => x.Type == "sub");
            if (sub == null)
                return null;

            if (!Guid.TryParse(sub.Value, out var accountId))
                return null;

            return accountId;
        }
    }
}
