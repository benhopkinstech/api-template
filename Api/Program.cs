using Api;
using Api.Modules.Identity.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;

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

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.Add("X-Retry-After", retryAfter.ToString());
        }

        await context.HttpContext.Response.WriteAsync("", cancellationToken: token);
    };
    options.AddPolicy("fw2req5m", httpContext => RateLimitExtensions.GetFixedWindowLimiterByEndpointAndIp(httpContext, 2, TimeSpan.FromMinutes(5)));
    options.AddPolicy("fw5req5m", httpContext => RateLimitExtensions.GetFixedWindowLimiterByEndpointAndIp(httpContext, 5, TimeSpan.FromMinutes(5)));
    options.AddPolicy("fw10req5m", httpContext => RateLimitExtensions.GetFixedWindowLimiterByEndpointAndIp(httpContext, 10, TimeSpan.FromMinutes(5)));
    options.AddPolicy("fw2req10m", httpContext => RateLimitExtensions.GetFixedWindowLimiterByEndpointAndIp(httpContext, 2, TimeSpan.FromMinutes(10)));
    options.AddPolicy("fw5req10m", httpContext => RateLimitExtensions.GetFixedWindowLimiterByEndpointAndIp(httpContext, 5, TimeSpan.FromMinutes(10)));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Authorization", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Token from Identity/Login",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

builder.Services.AddHttpContextAccessor();

builder.Services.RegisterModules();

var app = builder.Build();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

app.UseAuthentication();
app.UseAuthorization();

app.UseForwardedHeaders();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnableTryItOutByDefault());
}

app.UseHttpsRedirection();
app.MapEndpoints();

app.Run();

public partial class Program { }