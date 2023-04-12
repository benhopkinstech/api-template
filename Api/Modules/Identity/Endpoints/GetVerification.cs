using Api.Modules.Identity.Interfaces;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class GetVerification
    {
        public static async Task<IResult> VerifyAsync(string code, IIdentityService identity, IConfiguration config)
        {
            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlFail") ?? "");

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlFail") ?? "");

            if (!Guid.TryParse(decodedItems[0], out var verificationId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var verificationCreated))
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlFail") ?? "");

            if (DateTime.UtcNow > verificationCreated.AddHours(config.GetValue<int>("Identity:VerificationExpiryHours")))
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlFail") ?? "");

            var accountVerification = await identity.GetVerificationByIdAsync(verificationId);
            if (accountVerification == null)
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlFail") ?? "");

            var difference = accountVerification.CreatedOn - verificationCreated;
            if (accountVerification.Id != verificationId || accountVerification.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlFail") ?? "");

            var account = await identity.GetLocalAccountIncludeVerificationByIdAsync(accountId);
            if (account == null || account.Verification == null)
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlFail") ?? "");

            if (account.IsVerified)
            {
                await identity.RemoveRangeVerificationAsync(account.Verification);
                await identity.SaveChangesAsync();
                return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlSuccess") ?? "");
            }

            await identity.AmendAccountVerifiedAsync(account);
            await identity.RemoveRangeVerificationAsync(account.Verification);
            await identity.SaveChangesAsync();
            return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrlSuccess") ?? "");
        }
    }
}
