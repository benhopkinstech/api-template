using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostVerificationLink
    {
        [Authorize]
        public static async Task<IResult> SendVerificationLinkAsync(EmailModel email, IIdentityRepository identity, IConfiguration config, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountAndVerificationByIdAsync(accountId.Value);
            if (account == null || account.Verification == null)
                return Results.NotFound();

            if (account.Email != email.Email)
                return Results.NotFound();

            if (account.Verification.Count > 0)
                identity.RemoveRangeVerification(account.Verification);

            if (account.Verified)
                return Results.BadRequest("Account already verified");

            var verification = await identity.AddVerificationAsync(account.Id);
            await identity.SaveChangesAsync();

            if (!await Email.SendVerificationLinkAsync(config, http, verification, account.Email))
                return Results.BadRequest("Failed to send verification link");

            return Results.Ok("Verification link sent");
        }
    }
}
