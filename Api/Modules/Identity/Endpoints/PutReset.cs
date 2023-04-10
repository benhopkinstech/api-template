using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutReset
    {
        public static async Task<IResult> ResetAsync(PasswordModel newPassword, string code, IIdentityRepository identity)
        {
            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.NotFound();

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.NotFound();

            if (!Guid.TryParse(decodedItems[0], out var resetId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var resetCreated))
                return Results.NotFound();

            if (DateTime.UtcNow > resetCreated.AddDays(1))
                return Results.BadRequest("Reset link expired");

            var accountReset = await identity.GetResetByIdAsync(resetId);
            if (accountReset == null)
                return Results.NotFound();

            var difference = accountReset.CreatedOn - resetCreated;
            if (accountReset.Id != resetId || accountReset.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludePasswordResetByIdAsync(accountId);
            if (account == null || account.Password == null || account.Reset == null)
                return Results.NotFound();

            await identity.AmendPasswordAsync(account.Password, newPassword.Password);
            identity.RemoveRangeReset(account.Reset);
            await identity.SaveChangesAsync();

            return Results.Ok("Password reset");
        }
    }
}
