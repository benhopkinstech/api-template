using Api.Options;
using Microsoft.Extensions.Options;

namespace Api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddOptions<JwtOptions>().Bind(configuration.GetSection("Jwt"))
                .Validate(o => !String.IsNullOrWhiteSpace(o.TokenSecret))
                .Validate(o => !String.IsNullOrWhiteSpace(o.Issuer))
                .Validate(o => !String.IsNullOrWhiteSpace(o.Audience))
                .ValidateOnStart();
            services.AddOptions<SendGridOptions>().Bind(configuration.GetSection("SendGrid"))
                .Validate(o => !String.IsNullOrWhiteSpace(o.ApiKey))
                .Validate(o => !String.IsNullOrWhiteSpace(o.Email))
                .Validate(o => !String.IsNullOrWhiteSpace(o.VerificationLinkTemplateId))
                .Validate(o => !String.IsNullOrWhiteSpace(o.EmailChangedTemplateId))
                .Validate(o => !String.IsNullOrWhiteSpace(o.ResetLinkTemplateId))
                .Validate(o => !String.IsNullOrWhiteSpace(o.ResetUrl))
                .ValidateOnStart();
            services.AddOptions<IdentityOptions>().Bind(configuration.GetSection("Identity"))
                .Validate(o => !String.IsNullOrWhiteSpace(o.VerificationRedirectUrlSuccess))
                .Validate(o => !String.IsNullOrWhiteSpace(o.VerificationRedirectUrlFail))
                .ValidateOnStart();

            services.AddScoped(s => s.GetRequiredService<IOptionsMonitor<JwtOptions>>().CurrentValue);
            services.AddScoped(s => s.GetRequiredService<IOptionsMonitor<SendGridOptions>>().CurrentValue);
            services.AddScoped(s => s.GetRequiredService<IOptionsMonitor<IdentityOptions>>().CurrentValue);

            return services;
        }
    }
}
