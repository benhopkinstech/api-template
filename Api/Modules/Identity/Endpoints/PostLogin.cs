using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostLogin
    {
        public static async Task<IResult> LoginAsync(CredentialsModel credentials, IIdentityRepository identity, IConfiguration config, HttpContext http)
        {
            if (!await identity.AnyLocalAccountByEmailAsync(credentials.Email))
                return await NotFoundAsync(identity, credentials.Email, http);

            var account = await identity.GetLocalAccountIncludePasswordByEmailAsync(credentials.Email);
            if (account == null || account.Password == null)
                return await NotFoundAsync(identity, credentials.Email, http);

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
                return await UnauthorizedAsync(identity, account.Id, account.Email, http);

            if (config.GetValue<bool>("Identity:VerificationRequired") && account.IsVerified == false)
            {
                await InsertSuccessfulLoginAsync(identity, account.Id, account.Email, http);
                return Results.Forbid();
            }

            await InsertSuccessfulLoginAsync(identity, account.Id, account.Email, http);
            return Results.Content(Authorization.GenerateToken(config, account.Id, account.Email));
        }

        private async static Task<IResult> NotFoundAsync(IIdentityRepository identity, string email, HttpContext http)
        {
            await identity.AddLoginAsync(null, email, false, http);
            await identity.SaveChangesAsync();
            return Results.NotFound();
        }

        private async static Task<IResult> UnauthorizedAsync(IIdentityRepository identity, Guid accountId, string email, HttpContext http)
        {
            await identity.AddLoginAsync(accountId, email, false, http);
            await identity.SaveChangesAsync();
            return Results.Unauthorized();
        }

        private async static Task InsertSuccessfulLoginAsync(IIdentityRepository identity, Guid accountId, string email, HttpContext http)
        {
            await identity.AddLoginAsync(accountId, email, true, http);
            await identity.SaveChangesAsync();
        }
    }
}
