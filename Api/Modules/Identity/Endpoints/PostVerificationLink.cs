using Api.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostVerificationLink
    {
        [Authorize]
        public static async Task<IResult> SendVerificationLinkAsync(IIdentityService identity, IAuthService auth, IEmailService email)
        {
            var accountId = auth.GetAccountId();
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludeVerificationByIdAsync(accountId.Value);
            if (account == null)
                return Results.NotFound();

            if (account.IsVerified)
                return Results.Conflict();

            if (account.Verification != null && DateTime.UtcNow < account.Verification.CreatedOn.AddMinutes(10))
                if (!await email.SendVerificationLinkAsync(account.Verification, account.Email))
                    return Results.StatusCode(424);
                else
                    return Results.Ok();

            if (account.Verification != null)
                await identity.RemoveVerificationAsync(account.Verification);

            var verification = await identity.AddVerificationAsync(account.Id);
            await identity.SaveChangesAsync();

            if (!await email.SendVerificationLinkAsync(verification, account.Email))
                return Results.StatusCode(424);

            return Results.Ok();
        }
    }
}
