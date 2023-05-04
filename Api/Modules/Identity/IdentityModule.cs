using Api.Interfaces;
using Api.Modules.Identity.Endpoints;
using Api.Modules.Identity.Filters;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Services;

namespace Api.Modules.Identity
{
    public class IdentityModule : IModule
    {
        private readonly string _module = "Identity";

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IEncryptionService, EncryptionService>();

            return services;
        }

        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost($"{_module}/Register", PostRegister.RegisterAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .RequireRateLimiting("fw10req5m")
                .Produces(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status409Conflict).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PostRegister.RegisterAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/Login", PostLogin.LoginAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .RequireRateLimiting("fw10req5m")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status401Unauthorized).Produces(StatusCodes.Status403Forbidden).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PostLogin.LoginAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/Refresh", PostRefresh.RefreshAsync)
                .RequireRateLimiting("fw5req5m")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PostRefresh.RefreshAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Email", PutEmail.UpdateEmailAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .RequireRateLimiting("fw5req5m")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status403Forbidden).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PutEmail.UpdateEmailAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Password", PutPassword.UpdatePasswordAsync)
                .AddEndpointFilter<PasswordUpdateValidationFilter>()
                .RequireRateLimiting("fw5req5m")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status403Forbidden).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PutPassword.UpdatePasswordAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/VerificationLink", PostVerificationLink.SendVerificationLinkAsync)
                .RequireRateLimiting("fw2req5m")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict).Produces(StatusCodes.Status424FailedDependency).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PostVerificationLink.SendVerificationLinkAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/ResetLink", PostResetLink.SendResetLinkAsync)
                .AddEndpointFilter<EmailValidationFilter>()
                .RequireRateLimiting("fw2req10m")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status424FailedDependency).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PostResetLink.SendResetLinkAsync)).WithOpenApi();

            endpoints.MapGet($"{_module}/Verification", GetVerification.VerifyAsync)
                .RequireRateLimiting("fw5req10m")
                .Produces(StatusCodes.Status302Found)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(GetVerification.VerifyAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Reset", PutReset.ResetAsync)
                .AddEndpointFilter<PasswordValidationFilter>()
                .RequireRateLimiting("fw5req10m")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status410Gone).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PutReset.ResetAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Delete", PutDelete.DeleteAsync)
                .AddEndpointFilter<PasswordValidationFilter>()
                .RequireRateLimiting("fw5req10m")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status429TooManyRequests)
                .WithTags(_module).WithName(nameof(PutDelete.DeleteAsync)).WithOpenApi();

            return endpoints;
        }
    }
}
