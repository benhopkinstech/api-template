using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostVerificationLink
    {
        [Authorize]
        public static async Task<IResult> SendVerificationLinkAsync(EmailModel email, IdentityContext identity, HttpContext http, IConfiguration config)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Id == accountId).Include(x => x.Verification).FirstOrDefaultAsync();
            if (account == null || account.Verification == null)
                return Results.NotFound();

            if (account.Email != email.Email)
                return Results.NotFound();

            if (account.Verification.Count > 0)
                identity.Verification.RemoveRange(account.Verification);

            if (account.Verified)
                return Results.BadRequest("Account already verified");

            var verification = new Verification { Id = Guid.NewGuid(), AccountId = account.Id, CreatedOn = DateTime.UtcNow };

            await identity.Verification.AddAsync(verification);
            await identity.SaveChangesAsync();

            if (!await Email.SendVerificationLinkAsync(config, http, verification, account.Email))
                return Results.BadRequest("Failed to send verification link");

            return Results.Ok("Verification link sent");
        }
    }
}
