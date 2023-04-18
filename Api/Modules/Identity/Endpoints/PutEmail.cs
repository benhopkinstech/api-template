using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutEmail
    {
        [Authorize]
        public static async Task<IResult> UpdateEmailAsync(CredentialsModel credentials, IIdentityService identity, IAuthService auth, IEmailService email)
        {
            var accountId = auth.GetAccountId();
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludePasswordVerificationByIdAsync(accountId.Value);
            if (account == null || account.Password == null)
                return Results.NotFound();

            if (credentials.Email == account.Email)
                return Results.Conflict();

            if (await identity.AnyLocalAccountByEmailAsync(credentials.Email))
                return Results.Conflict();

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
                return Results.Forbid();

            if (account.Verification != null)
                await identity.RemoveVerificationAsync(account.Verification);

            var verification = await identity.AddVerificationAsync(account.Id);
            var currentEmail = account.Email;
            account = await identity.AmendAccountEmailAsync(account, credentials.Email);
            await identity.SaveChangesAsync();

            await email.SendEmailChangedAsync(currentEmail, account.Email);
            await email.SendVerificationLinkAsync(verification, account.Email);

            return Results.Ok();
        }
    }
}
