using Api.Settings;

namespace Api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddOptions<JwtSettings>().Bind(configuration.GetSection("Jwt"))
                .Validate(o => !String.IsNullOrWhiteSpace(o.TokenSecret))
                .Validate(o => !String.IsNullOrWhiteSpace(o.Issuer))
                .Validate(o => !String.IsNullOrWhiteSpace(o.Audience))
                .ValidateOnStart();
            services.AddOptions<SendGridSettings>().Bind(configuration.GetSection("SendGrid"))
                .Validate(o => !String.IsNullOrWhiteSpace(o.ApiKey))
                .Validate(o => !String.IsNullOrWhiteSpace(o.Email))
                .Validate(o => !String.IsNullOrWhiteSpace(o.VerificationLinkTemplateId))
                .Validate(o => !String.IsNullOrWhiteSpace(o.EmailChangedTemplateId))
                .Validate(o => !String.IsNullOrWhiteSpace(o.ResetLinkTemplateId))
                .Validate(o => !String.IsNullOrWhiteSpace(o.ResetUrl))
                .ValidateOnStart();
            services.AddOptions<IdentitySettings>().Bind(configuration.GetSection("Identity"))
                .Validate(o => !String.IsNullOrWhiteSpace(o.VerificationRedirectUrlSuccess))
                .Validate(o => !String.IsNullOrWhiteSpace(o.VerificationRedirectUrlFail))
                .ValidateOnStart();

            return services;
        }
    }
}
