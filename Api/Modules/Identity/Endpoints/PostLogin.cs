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
            {
                await identity.InsertLoginAsync(null, credentials.Email, false, http);
                return Results.Unauthorized();
            }

            var account = await identity.GetLocalAccountIncludePasswordByEmailAsync(credentials.Email);
            if (account == null || account.Password == null)
            {
                await identity.InsertLoginAsync(null, credentials.Email, false, http);
                return Results.Unauthorized();
            }

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
            {
                await identity.InsertLoginAsync(account.Id, account.Email, false, http);
                return Results.Unauthorized();
            }

            if (config.GetValue<bool>("Identity:VerificationRequired") && account.Verified == false)
            {
                await identity.InsertLoginAsync(account.Id, account.Email, true, http);
                return Results.Forbid();
            }

            await identity.InsertLoginAsync(account.Id, account.Email, true, http);
            return Results.Content(Authorization.GenerateToken(config, account.Id, account.Email));
        }
    }
}
