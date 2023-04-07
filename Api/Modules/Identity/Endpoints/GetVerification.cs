﻿using Api.Modules.Identity.Data;
using Api.Modules.Identity.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class GetVerification
    {
        public static async Task<IResult> VerifyAsync(string code, IIdentityRepository identity, IConfiguration config)
        {
            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.NotFound();

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.NotFound();

            if (!Guid.TryParse(decodedItems[0], out var verificationId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var verificationCreated))
                return Results.NotFound();

            if (DateTime.UtcNow > verificationCreated.AddDays(3))
                return Results.BadRequest("Verification link expired");

            var accountVerification = await identity.GetVerificationByIdAsync(verificationId);
            if (accountVerification == null)
                return Results.NotFound();

            var difference = accountVerification.CreatedOn - verificationCreated;
            if (accountVerification.Id != verificationId || accountVerification.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.NotFound();

            var account = await identity.GetLocalAccountAndVerificationByIdAsync(accountId);
            if (account == null)
                return Results.NotFound();

            if (account.Verified)
            {
                identity.RemoveRangeVerification(account.Verification);
                await identity.SaveChangesAsync();
                return Results.Ok("Account already verified");
            }

            identity.AmendAccountVerified(account);
            identity.RemoveRangeVerification(account.Verification);
            await identity.SaveChangesAsync();
            return Results.Redirect(config.GetValue<string>("Identity:VerificationRedirectUrl") ?? "");
        }
    }
}
