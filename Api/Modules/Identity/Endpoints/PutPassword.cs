using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutPassword
    {
        [Authorize]
        public static async Task<IResult> UpdatePasswordAsync(PasswordUpdateModel passwordUpdate, IdentityContext identity, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Id == accountId).Include(x => x.Password).FirstOrDefaultAsync();
            if (account == null || account.Password == null)
                return Results.NotFound();

            var correctPassword = Encryption.VerifyHash(passwordUpdate.CurrentPassword, account.Password.Hash);
            if (!correctPassword)
                return Results.NotFound();

            var currentPassword = account.Password.Hash;
            account.Password.Hash = Encryption.GenerateHash(passwordUpdate.Password);
            account.Password.UpdatedOn = DateTime.UtcNow;
            await identity.PasswordAudit.AddAsync(new PasswordAudit { AccountId = account.Id, Hash = currentPassword });
            await identity.SaveChangesAsync();

            return Results.Ok("Password updated");
        }
    }
}
