using Api.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostVerificationLink
    {
        [Authorize]
        public static async Task<IResult> SendVerificationLinkAsync(IIdentityService identity, IUserService user, IEmailService email, IConfiguration config)
        {
            var accountId = user.GetAccountId();
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetLocalAccountIncludeVerificationByIdAsync(accountId.Value);
            if (account == null)
                return Results.NotFound();

            if (account.IsVerified)
                return Results.Conflict();

            if (account.Verification != null && DateTime.UtcNow < account.Verification.CreatedOn.AddMinutes(config.GetValue<int>("Identity:VerificationResendLimitMinutes")))
                return Results.StatusCode(429);

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
