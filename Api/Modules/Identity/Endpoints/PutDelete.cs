using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutDelete
    {
        [Authorize]
        public static async Task<IResult> DeleteAsync(PasswordModel delete, IIdentityService identity, IAuthService auth, IEncryptionService encryption)
        {
            var accountId = auth.GetAccountId();
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetAccountIncludeAllByIdAsync(accountId.Value);
            if (account == null || account.PasswordAudit == null || account.AccountAudit == null || account.Login == null || account.Refresh == null || account.Password == null)
                return Results.NotFound();

            var correctPassword = encryption.VerifyHash(delete.Password, account.Password.Hash);
            if (!correctPassword)
                return Results.Forbid();

            await identity.RemoveAll(account.PasswordAudit, account.AccountAudit, account.Login, account.Refresh, account.Verification, account.Reset, account.Password, account);
            await identity.SaveChangesAsync();
            return Results.NoContent();
        }
    }
}
