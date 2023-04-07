using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;

namespace Api.Modules.Identity.Filters
{
    public class EmailValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var model = context.GetArgument<IEmail>(0);
            var errors = new List<string>();

            errors.AddRange(Validation.EmailCheck(model.Email));

            if (errors.Count > 0)
                return Results.BadRequest(errors);

            return await next(context);
        }
    }
}
