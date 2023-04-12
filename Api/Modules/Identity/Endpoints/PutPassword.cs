using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutPassword
    {
        [Authorize]
        public static async Task<IResult> UpdatePasswordAsync(PasswordUpdateModel update, IIdentityRepository identity, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludePasswordResetByIdAsync(accountId.Value);
            if (account == null || account.Password == null || account.Reset == null)
                return Results.NotFound();

            var correctPassword = Encryption.VerifyHash(update.CurrentPassword, account.Password.Hash);
            if (!correctPassword)
                return Results.Forbid();

            if (account.Verification.Count > 0)
                await identity.RemoveRangeResetAsync(account.Reset);

            await identity.AmendPasswordAsync(account.Password, update.Password);
            await identity.SaveChangesAsync();

            return Results.Ok();
        }
    }
}
