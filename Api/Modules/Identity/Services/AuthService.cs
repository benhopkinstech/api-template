using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Interfaces;
using Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Modules.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _http;
        private readonly JwtSettings _settings;
        private readonly JwtSettings _settingsCurrent;

        public AuthService(IHttpContextAccessor http, IOptions<JwtSettings> settings, IOptionsMonitor<JwtSettings> settingsCurrent)
        {
            _http = http;
            _settings = settings.Value;
            _settingsCurrent = settingsCurrent.CurrentValue;
        }

        public string GenerateTokens(Guid accountId, string email, Refresh refresh)
        {
            var token = GenerateToken(accountId, email);
            SetRefreshToken(refresh);
            return token;
        }

        private string GenerateToken(Guid accountId, string email)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("sub", accountId.ToString()),
                new Claim("email", email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.TokenSecret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: null,
                expires: DateTime.UtcNow.AddMinutes(_settingsCurrent.TokenExpiryMinutes),
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
