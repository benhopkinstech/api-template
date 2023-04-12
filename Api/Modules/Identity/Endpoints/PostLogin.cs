using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostLogin
    {
        public static async Task<IResult> LoginAsync(CredentialsModel credentials, IIdentityService identity, IUserService user, IConfiguration config)
        {
            if (!await identity.AnyLocalAccountByEmailAsync(credentials.Email))
                return await NotFoundAsync(identity, credentials.Email);

            var account = await identity.GetLocalAccountIncludePasswordByEmailAsync(credentials.Email);
            if (account == null || account.Password == null)
                return await NotFoundAsync(identity, credentials.Email);

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
                return await UnauthorizedAsync(identity, account.Id, account.Email);

            if (config.GetValue<bool>("Identity:VerificationRequired") && account.IsVerified == false)
            {
                await InsertSuccessfulLoginAsync(identity, account.Id, account.Email);
                return Results.Forbid();
            }

            await InsertSuccessfulLoginAsync(identity, account.Id, account.Email);
            return Results.Content(user.GenerateToken(account.Id, account.Email));
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

        private async static Task InsertSuccessfulLoginAsync(IIdentityService identity, Guid accountId, string email)
        {
            await identity.AddLoginAsync(accountId, email, true);
            await identity.SaveChangesAsync();
        }
    }
}
