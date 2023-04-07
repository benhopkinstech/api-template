using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutReset
    {
        public static async Task<IResult> ResetAsync(string code, PasswordModel newPassword, IdentityContext identity)
        {
            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.NotFound();

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.NotFound();

            if (!Guid.TryParse(decodedItems[0], out var resetId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var resetCreated))
                return Results.NotFound();

            if (DateTime.UtcNow > resetCreated.AddDays(3))
                return Results.BadRequest("Reset link expired");

            var accountReset = await identity.Reset.Where(x => x.Id == resetId).FirstOrDefaultAsync();
            if (accountReset == null)
                return Results.NotFound();

            var difference = accountReset.CreatedOn - resetCreated;
            if (accountReset.Id != resetId || accountReset.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.NotFound();

            var account = await identity.Account.Where(x => x.Id == accountId).Include(x => x.Password).Include(x => x.Reset).FirstOrDefaultAsync();
            if (account == null || account.Password == null)
                return Results.NotFound();

            var currentPassword = account.Password.Hash;
            account.Password.Hash = Encryption.GenerateHash(newPassword.Password);
            account.Password.UpdatedOn = DateTime.UtcNow;
            identity.Reset.RemoveRange(account.Reset);
            await identity.PasswordAudit.AddAsync(new PasswordAudit { AccountId = account.Id, Hash = currentPassword });
            await identity.SaveChangesAsync();

            return Results.Ok("Password reset");
        }
    }
}
