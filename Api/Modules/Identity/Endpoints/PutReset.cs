﻿using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Api.Options;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class PutReset
    {
        public static async Task<IResult> ResetAsync(PasswordModel reset, string code, IIdentityService identity, IdentityOptions options)
        {
            if (!Convert.TryFromBase64String(code, new byte[code.Length], out _))
                return Results.NotFound();

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(code)).Split('&');
            if (decodedItems.Length != 3)
                return Results.NotFound();

            if (!Guid.TryParse(decodedItems[0], out var resetId) || !Guid.TryParse(decodedItems[1], out var accountId) || !DateTime.TryParse(decodedItems[2], out var resetCreated))
                return Results.NotFound();

            if (DateTime.UtcNow > resetCreated.AddHours(options.ResetExpiryHours))
                return Results.StatusCode(410);

            var accountReset = await identity.GetResetByIdAsync(resetId);
            if (accountReset == null)
                return Results.NotFound();

            var difference = accountReset.CreatedOn - resetCreated;
            if (accountReset.Id != resetId || accountReset.AccountId != accountId || difference > TimeSpan.FromSeconds(3))
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludePasswordByIdAsync(accountId);
            if (account == null || account.Password == null)
                return Results.NotFound();

            await identity.AmendPasswordAsync(account.Password, reset.Password);
            await identity.RemoveResetAsync(accountReset);
            await identity.SaveChangesAsync();

            return Results.Ok();
        }
    }
}
