using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutEmail
    {
        [Authorize]
        public static async Task<IResult> UpdateEmailAsync(CredentialsModel credentials, IIdentityRepository identity, IConfiguration config, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludePasswordVerificationByIdAsync(accountId.Value);
            if (account == null || account.Password == null || account.Verification == null)
                return Results.NotFound();

            if (await identity.AnyLocalAccountByEmailAsync(credentials.Email))
                return Results.Conflict("Email already in use");

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
                return Results.NotFound();

            if (account.Verification.Count > 0)
                await identity.RemoveRangeVerification(account.Verification);

            var verification = await identity.AddVerificationAsync(account.Id);
            var currentEmail = account.Email;
            account = await identity.AmendAccountEmailAsync(account, credentials.Email);
            await identity.SaveChangesAsync();

            await Email.SendEmailChangedAsync(config, currentEmail, account.Email);
            await Email.SendVerificationLinkAsync(config, http, verification, account.Email);

            return Results.Ok("Email updated");
        }
    }
}
