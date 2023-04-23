using Api.Modules.Identity.Interfaces;
using Api.Settings;
using Microsoft.Extensions.Options;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class GetVerification
    {
        public static async Task<IResult> VerifyAsync(string code, IIdentityService identity, IOptionsMonitor<IdentitySettings> settings)
        {
            string verificationRedirectUrlFail = settings.CurrentValue.VerificationRedirectUrlFail;
            string verificationRedirectUrlSuccess = settings.CurrentValue.VerificationRedirectUrlSuccess;

            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.Redirect(verificationRedirectUrlFail);

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.Redirect(verificationRedirectUrlFail);

            if (!Guid.TryParse(decodedItems[0], out var verificationId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var verificationCreated))
                return Results.Redirect(verificationRedirectUrlFail);

            if (DateTime.UtcNow > verificationCreated.AddHours(settings.CurrentValue.VerificationExpiryHours))
                return Results.Redirect(verificationRedirectUrlFail);

            var accountVerification = await identity.GetVerificationByIdAsync(verificationId);
            if (accountVerification == null)
                return Results.Redirect(verificationRedirectUrlFail);

            var difference = accountVerification.CreatedOn - verificationCreated;
            if (accountVerification.Id != verificationId || accountVerification.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.Redirect(verificationRedirectUrlFail);

            var account = await identity.GetLocalAccountByIdAsync(accountId);
            if (account == null)
                return Results.Redirect(verificationRedirectUrlFail);

            if (account.IsVerified)
            {
                await identity.RemoveVerificationAsync(accountVerification);
                await identity.SaveChangesAsync();
                return Results.Redirect(verificationRedirectUrlSuccess);
            }

            await identity.AmendAccountVerifiedAsync(account);
            await identity.RemoveVerificationAsync(accountVerification);
            await identity.SaveChangesAsync();
            return Results.Redirect(verificationRedirectUrlSuccess);
        }
    }
}
