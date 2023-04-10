﻿using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostVerificationLink
    {
        [Authorize]
        public static async Task<IResult> SendVerificationLinkAsync(IIdentityRepository identity, IConfiguration config, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountByIdAsync(accountId.Value);
            if (account == null)
                return Results.NotFound();

            if (account.Verified)
                return Results.BadRequest("Account already verified");

            var verification = await identity.AddVerificationAsync(account.Id);
            await identity.SaveChangesAsync();

            if (!await Email.SendVerificationLinkAsync(config, http, verification, account.Email))
                return Results.BadRequest("Failed to send verification link");

            return Results.Ok("Verification link sent");
        }
    }
}
