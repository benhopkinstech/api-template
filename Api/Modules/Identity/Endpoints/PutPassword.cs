using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutPassword
    {
        [Authorize]
        public static async Task<IResult> UpdatePasswordAsync(PasswordUpdateModel passwordUpdate, IIdentityRepository identity, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountAndPasswordByIdAsync(accountId.Value);
            if (account == null || account.Password == null)
                return Results.NotFound();

            var correctPassword = Encryption.VerifyHash(passwordUpdate.CurrentPassword, account.Password.Hash);
            if (!correctPassword)
                return Results.NotFound();

            await identity.AmendPasswordAsync(account.Password, passwordUpdate.Password);
            await identity.SaveChangesAsync();

            return Results.Ok("Password updated");
        }
    }
}
