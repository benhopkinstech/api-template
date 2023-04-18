using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Modules.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _http;
        private readonly IConfiguration _config;

        public AuthService(IHttpContextAccessor http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public string GenerateTokens(Guid accountId, string email, Refresh refresh)
        {
            var token = GenerateToken(accountId, email);
            SetRefreshToken(refresh);
            return token;
        }

        private string GenerateToken(Guid accountId, string email)
        {
            string? tokenSecret = _config.GetValue<string>("Jwt:TokenSecret");

            if (tokenSecret == null)
            {
                return "";
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim("sub", accountId.ToString()),
                new Claim("email", email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config.GetValue<string>("Jwt:Issuer"),
                audience: _config.GetValue<string>("Jwt:Audience"),
                claims: claims,
                notBefore: null,
                expires: DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:TokenValidityMinutes")),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void SetRefreshToken(Refresh refresh)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refresh.ExpiresOn
            };
            _http.HttpContext?.Response.Cookies.Append("refreshToken", Convert.ToBase64String(Encoding.Unicode.GetBytes($"{refresh.Id}&{refresh.Secret}")), cookieOptions);
        }

        public Guid? GetAccountId()
        {
            if (_http.HttpContext?.User.Claims == null)
                return null;

            var sub = _http.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "sub");
            if (sub == null)
                return null;

            if (!Guid.TryParse(sub.Value, out var accountId))
                return null;

            return accountId;
        }
    }
}
