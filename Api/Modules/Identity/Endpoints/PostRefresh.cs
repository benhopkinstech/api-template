using Api.Modules.Identity.Interfaces;
using System.Text;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostRefresh
    {
        public static async Task<IResult> RefreshAsync(IIdentityService identity, IAuthService auth, IConfiguration config, HttpContext http)
        {
            var token = http.Request.Cookies["refreshToken"] ?? "";

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
                var notUsedRefreshRecords = await identity.GetRefreshListNotUsedByAccountIdAsync(refresh.AccountId);
                foreach (var record in notUsedRefreshRecords)
                    await identity.AmendRefreshUsedAsync(record);
                await identity.SaveChangesAsync();
                return Results.NotFound();
            }

            await identity.AmendRefreshUsedAsync(refresh);
            var newRefresh = await identity.AddRefreshAsync(refresh.AccountId, DateTime.UtcNow.AddHours(config.GetValue<int>("Jwt:RefreshExpiryHours")));
            await identity.SaveChangesAsync();
            
            return Results.Content(auth.GenerateTokens(refresh.AccountId, refresh.Account.Email, newRefresh));
        }
    }
}
