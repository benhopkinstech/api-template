using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutPassword
    {
        [Authorize]
        public static async Task<IResult> UpdatePasswordAsync(PasswordUpdateModel update, IIdentityService identity, IAuthService auth, IEncryptionService encryption)
        {
            var accountId = auth.GetAccountId();
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludePasswordResetByIdAsync(accountId.Value);
            if (account == null || account.Password == null)
                return Results.NotFound();

            var correctPassword = encryption.VerifyHash(update.CurrentPassword, account.Password.Hash);
            if (!correctPassword)
                return Results.Forbid();

            if (account.Reset != null)
                await identity.RemoveResetAsync(account.Reset);

            await identity.AmendPasswordAsync(account.Password, update.Password);
            await identity.SaveChangesAsync();

            return Results.Ok();
        }
    }
}
