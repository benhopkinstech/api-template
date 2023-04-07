using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;

namespace Api.Modules.Identity.Filters
{
    public class CredentialsValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var model = context.GetArgument<ICredentials>(0);
            var errors = new List<string>();

            errors.AddRange(Validation.EmailCheck(model.Email));

            errors.AddRange(Validation.PasswordCheck(model.Password));

            if (errors.Count > 0)
                return Results.BadRequest(errors);

            return await next(context);
        }
    }
}
