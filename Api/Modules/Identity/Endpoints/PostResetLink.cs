using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostResetLink
    {
        public static async Task<IResult> SendResetLinkAsync(EmailModel email, IIdentityRepository identity, IConfiguration config)
        {
            var account = await identity.GetLocalAccountAndResetByEmailAsync(email.Email);
            if (account == null || account.Reset == null)
                return Results.NotFound();

            if (account.Reset.Count > 0)
                identity.RemoveRangeReset(account.Reset);

            var reset = await identity.AddResetAsync(account.Id);
            await identity.SaveChangesAsync();

            if (!await Email.SendResetLinkAsync(config, reset, account.Email))
                return Results.BadRequest("Failed to send reset link");

            return Results.Ok("Reset link sent");
        }
    }
}
