using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Models;
using System.Text.Json;

namespace Api.Modules.Identity.Filters
{
    public class PasswordUpdateValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var model = context.GetArgument<PasswordUpdateModel>(0);
            var errors = new Dictionary<string, string[]>();

            var passwordErrors = Validation.PasswordCheck(model.Password);
            if (passwordErrors.Length > 0)
                errors.Add(JsonNamingPolicy.CamelCase.ConvertName(nameof(model.Password)), passwordErrors);

            var currentPasswordErrors = Validation.PasswordCheck(model.CurrentPassword);
            if (currentPasswordErrors.Length > 0)
                errors.Add(JsonNamingPolicy.CamelCase.ConvertName(nameof(model.CurrentPassword)), currentPasswordErrors);

            if (model.Password == model.CurrentPassword)
                errors.Add("passwords", new string[] { "Must not match" });

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            return await next(context);
        }
    }
}
