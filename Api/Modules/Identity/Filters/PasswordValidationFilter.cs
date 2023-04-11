using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using System.Text.Json;

namespace Api.Modules.Identity.Filters
{
    public class PasswordValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var model = context.GetArgument<IPassword>(0);
            var errors = new Dictionary<string, string[]>();

            var passwordErrors = Validation.PasswordCheck(model.Password);
            if (passwordErrors.Length > 0)
                errors.Add(JsonNamingPolicy.CamelCase.ConvertName(nameof(model.Password)), passwordErrors);

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            return await next(context);
        }
    }
}
