using Api.Modules.Identity.Endpoints;
using Api.Modules.Identity.Filters;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Repositories;

namespace Api.Modules.Identity
{
    public class IdentityModule : IModule
    {
        private readonly string _module = "Identity";

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            services.AddScoped<IIdentityRepository, IdentityRepository>();

            return services;
        }

        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost($"{_module}/Register", PostRegister.RegisterAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .Produces(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status409Conflict)
                .WithTags(_module).WithName(nameof(PostRegister.RegisterAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/Login", PostLogin.LoginAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status401Unauthorized).Produces(StatusCodes.Status403Forbidden).Produces(StatusCodes.Status404NotFound)
                .WithTags(_module).WithName(nameof(PostLogin.LoginAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Email", PutEmail.UpdateEmailAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status403Forbidden).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict)
                .WithTags(_module).WithName(nameof(PutEmail.UpdateEmailAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Password", PutPassword.UpdatePasswordAsync)
                .AddEndpointFilter<PasswordUpdateValidationFilter>()
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status403Forbidden).Produces(StatusCodes.Status404NotFound)
                .WithTags(_module).WithName(nameof(PutPassword.UpdatePasswordAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/VerificationLink", PostVerificationLink.SendVerificationLinkAsync)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict).Produces(StatusCodes.Status424FailedDependency)
                .WithTags(_module).WithName(nameof(PostVerificationLink.SendVerificationLinkAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/ResetLink", PostResetLink.SendResetLinkAsync)
                .AddEndpointFilter<EmailValidationFilter>()
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status424FailedDependency)
                .WithTags(_module).WithName(nameof(PostResetLink.SendResetLinkAsync)).WithOpenApi();

            endpoints.MapGet($"{_module}/Verification", GetVerification.VerifyAsync)
                .Produces(StatusCodes.Status302Found)
                .WithTags(_module).WithName(nameof(GetVerification.VerifyAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Reset", PutReset.ResetAsync)
                .AddEndpointFilter<PasswordValidationFilter>()
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status410Gone)
                .WithTags(_module).WithName(nameof(PutReset.ResetAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Delete", PutDelete.DeleteAsync)
                .AddEndpointFilter<PasswordValidationFilter>()
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status404NotFound)
                .WithTags(_module).WithName(nameof(PutDelete.DeleteAsync)).WithOpenApi();

            return endpoints;
        }
    }
}
