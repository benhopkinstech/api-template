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
                .WithTags(_module).WithName(nameof(PostRegister.RegisterAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/Login", PostLogin.LoginAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .WithTags(_module).WithName(nameof(PostLogin.LoginAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Email", PutEmail.UpdateEmailAsync)
                .AddEndpointFilter<CredentialsValidationFilter>()
                .WithTags(_module).WithName(nameof(PutEmail.UpdateEmailAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Password", PutPassword.UpdatePasswordAsync)
                .AddEndpointFilter<PasswordUpdateValidationFilter>()
                .WithTags(_module).WithName(nameof(PutPassword.UpdatePasswordAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/VerificationLink", PostVerificationLink.SendVerificationLinkAsync)
                .WithTags(_module).WithName(nameof(PostVerificationLink.SendVerificationLinkAsync)).WithOpenApi();

            endpoints.MapPost($"{_module}/ResetLink", PostResetLink.SendResetLinkAsync)
                .AddEndpointFilter<EmailValidationFilter>()
                .WithTags(_module).WithName(nameof(PostResetLink.SendResetLinkAsync)).WithOpenApi();

            endpoints.MapGet($"{_module}/Verification", GetVerification.VerifyAsync)
                .WithTags(_module).WithName(nameof(GetVerification.VerifyAsync)).WithOpenApi();

            endpoints.MapPut($"{_module}/Reset", PutReset.ResetAsync)
                .AddEndpointFilter<PasswordValidationFilter>()
                .WithTags(_module).WithName(nameof(PutReset.ResetAsync)).WithOpenApi();

            endpoints.MapDelete($"{_module}/Delete", Delete.DeleteAsync)
                .WithTags(_module).WithName(nameof(Delete.DeleteAsync)).WithOpenApi();

            return endpoints;
        }
    }
}
