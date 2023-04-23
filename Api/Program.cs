using Api;
using Api.Modules.Identity.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IdentityContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string? tokenSecret = builder.Configuration.GetValue<string>("Jwt:TokenSecret");

        if (tokenSecret != null)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                ValidateAudience = true,
                ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret)),
                ValidateLifetime = true,
            };
        }
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiting();

builder.Services.AddSwagger();

builder.Services.AddHttpContextAccessor();

builder.Services.RegisterModules();

var app = builder.Build();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimting();

app.UseSwaggerIfDevelopment();

app.UseHttpsRedirection();
app.MapEndpoints();
app.Run();

public partial class Program { }