using Api.Modules.Identity.Interfaces;
using Api.Options;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostRefresh
    {
        public static async Task<IResult> RefreshAsync([FromHeader(Name = "X-Refresh-Token")] string token, IIdentityService identity, IAuthService auth, JwtOptions options)
        {
            if (!Convert.TryFromBase64String(token, new byte[token.Length], out _))
                return Results.NotFound();

            var decodedItems = Encoding.Unicode.GetString(Convert.FromBase64String(token)).Split('&');
            if (decodedItems.Length != 2)
                return Results.NotFound();

            if (!Guid.TryParse(decodedItems[0], out var refreshId))
                return Results.NotFound();

            string refreshSecret = decodedItems[1];
            var refresh = await identity.GetRefreshIncludeAccountByIdAsync(refreshId);
            if (refresh == null || refresh.Account == null || refresh.Secret != refreshSecret)
                return Results.NotFound();

            if (DateTime.UtcNow > refresh.ExpiresOn)
                return Results.NotFound();

            if (refresh.IsUsed)
            {
                var refreshRecords = await identity.GetRefreshListNotExpiredNotUsedByAccountIdAsync(refresh.AccountId);
                foreach (var record in refreshRecords)
                    await identity.AmendRefreshUsedAsync(record);
                await identity.SaveChangesAsync();
                return Results.NotFound();
            }

            await identity.AmendRefreshUsedAsync(refresh);
            var newRefresh = await identity.AddRefreshAsync(refresh.AccountId, DateTime.UtcNow.AddHours(options.RefreshExpiryHours));
            await identity.SaveChangesAsync();
            
            return Results.Content(auth.GenerateTokens(refresh.AccountId, refresh.Account.Email, newRefresh));
        }
    }
}
