using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using System.Text.Json;

namespace Api.Modules.Identity.Filters
{
    public class CredentialsValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var model = context.GetArgument<ICredentials>(0);
            var errors = new Dictionary<string, string[]>();

            var emailErrors = Validation.EmailCheck(model.Email);
            if (emailErrors.Length > 0)
                errors.Add(JsonNamingPolicy.CamelCase.ConvertName(nameof(model.Email)), emailErrors);

            var passwordErrors = Validation.PasswordCheck(model.Password);
            if (passwordErrors.Length > 0)
                errors.Add(JsonNamingPolicy.CamelCase.ConvertName(nameof(model.Password)), passwordErrors);

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            return await next(context);
        }
    }
}
