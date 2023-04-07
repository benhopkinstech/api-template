using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutEmail
    {
        [Authorize]
        public static async Task<IResult> UpdateEmailAsync(CredentialsModel credentials, IdentityContext identity, HttpContext http, IConfiguration config)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Id == accountId).Include(x => x.Password).Include(x => x.Verification).FirstOrDefaultAsync();
            if (account == null || account.Password == null || account.Verification == null)
                return Results.NotFound();

            if (await identity.Account.AnyAsync(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == credentials.Email))
                return Results.Conflict("Email already in use");

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
                return Results.NotFound();

            if (account.Verification.Count > 0)
                identity.Verification.RemoveRange(account.Verification);

            var verification = new Verification { Id = Guid.NewGuid(), AccountId = account.Id, CreatedOn = DateTime.UtcNow };
            var currentEmail = account.Email;
            account.Email = credentials.Email;
            account.Verified = false;
            account.VerifiedOn = null;
            account.UpdatedOn = DateTime.UtcNow;

            await identity.AccountAudit.AddAsync(new AccountAudit { AccountId = account.Id, Email = currentEmail });
            await identity.Verification.AddAsync(verification);
            await identity.SaveChangesAsync();

            _ = await Email.SendEmailChangedAsync(config, currentEmail, account.Email);
            _ = await Email.SendVerificationLinkAsync(config, http, verification, account.Email);

            return Results.Ok("Email updated");
        }
    }
}
