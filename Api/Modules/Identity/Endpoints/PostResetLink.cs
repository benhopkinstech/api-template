using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostResetLink
    {
        public static async Task<IResult> SendResetLinkAsync(EmailModel resetLink, IIdentityService identity, IEmailService email)
        {
            var account = await identity.GetLocalAccountByEmailAsync(resetLink.Email);
            if (account == null)
                return Results.NotFound();

            var reset = await identity.AddResetAsync(account.Id);
            await identity.SaveChangesAsync();

            if (!await email.SendResetLinkAsync(reset, account.Email))
                return Results.StatusCode(424);

            return Results.Ok();
        }
    }
}
