using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Models;

namespace Api.Modules.Identity.Filters
{
    public class PasswordUpdateValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var model = context.GetArgument<PasswordUpdateModel>(0);
            var errors = new List<string>();

            errors.AddRange(Validation.PasswordCheck(model.Password));

            errors.AddRange(Validation.PasswordCheck(model.CurrentPassword));

            if (errors.Count == 0 && model.Password == model.CurrentPassword)
                errors.Add("Please ensure that the passwords provided are different");

            if (errors.Count > 0)
                return Results.BadRequest(errors);

            return await next(context);
        }
    }
}
