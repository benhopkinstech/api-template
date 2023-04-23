﻿using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;
using Api.Settings;
using Microsoft.Extensions.Options;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostLogin
    {
        public static async Task<IResult> LoginAsync(CredentialsModel credentials, IIdentityService identity, IAuthService auth, IOptionsMonitor<IdentitySettings> identitySettings, IOptionsMonitor<JwtSettings> jwtSettings)
        {
            if (!await identity.AnyLocalAccountByEmailAsync(credentials.Email))
                return await NotFoundAsync(identity, credentials.Email);

            var account = await identity.GetLocalAccountIncludePasswordByEmailAsync(credentials.Email);
            if (account == null || account.Password == null)
                return await NotFoundAsync(identity, credentials.Email);

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
                return await UnauthorizedAsync(identity, account.Id, account.Email);

            if (identitySettings.CurrentValue.VerificationRequired && account.IsVerified == false)
            {
                await identity.AddLoginAsync(account.Id, account.Email, true);
                await identity.SaveChangesAsync();
                return Results.Forbid();
            }

            await identity.AddLoginAsync(account.Id, account.Email, true);
            var refresh = await identity.AddRefreshAsync(account.Id, DateTime.UtcNow.AddHours(jwtSettings.CurrentValue.RefreshExpiryHours));
            await identity.SaveChangesAsync();
            return Results.Content(auth.GenerateTokens(account.Id, account.Email, refresh));
        }

        private async static Task<IResult> NotFoundAsync(IIdentityService identity, string email)
        {
            await identity.AddLoginAsync(null, email, false);
            await identity.SaveChangesAsync();
            return Results.NotFound();
        }

        private async static Task<IResult> UnauthorizedAsync(IIdentityService identity, Guid accountId, string email)
        {
            await identity.AddLoginAsync(accountId, email, false);
            await identity.SaveChangesAsync();
            return Results.Unauthorized();
        }
    }
}
