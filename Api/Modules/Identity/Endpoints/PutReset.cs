using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutReset
    {
        public static async Task<IResult> ResetAsync(PasswordModel reset, string code, IIdentityService identity, IConfiguration config)
        {
            string resetRedirectUrlFail = config.GetValue<string>("Identity:ResetRedirectUrlFail") ?? "";
            string resetRedirectUrlSuccess = config.GetValue<string>("Identity:ResetRedirectUrlSuccess") ?? "";

            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.Redirect(resetRedirectUrlFail);

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.Redirect(resetRedirectUrlFail);

            if (!Guid.TryParse(decodedItems[0], out var resetId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var resetCreated))
                return Results.Redirect(resetRedirectUrlFail);

            if (DateTime.UtcNow > resetCreated.AddHours(config.GetValue<int>("Identity:ResetExpiryHours")))
                return Results.Redirect(resetRedirectUrlFail);

            var accountReset = await identity.GetResetByIdAsync(resetId);
            if (accountReset == null)
                return Results.Redirect(resetRedirectUrlFail);

            var difference = accountReset.CreatedOn - resetCreated;
            if (accountReset.Id != resetId || accountReset.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.Redirect(resetRedirectUrlFail);

            var account = await identity.GetLocalAccountIncludePasswordByIdAsync(accountId);
            if (account == null || account.Password == null)
                return Results.Redirect(resetRedirectUrlFail);

            await identity.AmendPasswordAsync(account.Password, reset.Password);
            await identity.RemoveResetAsync(accountReset);
            await identity.SaveChangesAsync();

            return Results.Redirect(resetRedirectUrlSuccess);
        }
    }
}
