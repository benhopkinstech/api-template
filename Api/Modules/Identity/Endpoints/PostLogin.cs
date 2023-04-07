using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostLogin
    {
        public static async Task<IResult> LoginAsync(CredentialsModel credentials, IdentityContext identity, HttpContext http, IConfiguration config)
        {
            if (!await identity.Account.AnyAsync(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == credentials.Email))
                return await AddLoginAsync(identity, http, config, null, credentials.Email, false);

            var account = await identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == credentials.Email).Include(x => x.Password).FirstOrDefaultAsync();
            if (account == null || account.Password == null)
                return await AddLoginAsync(identity, http, config, null, credentials.Email, false);

            var correctPassword = Encryption.VerifyHash(credentials.Password, account.Password.Hash);
            if (!correctPassword)
                return await AddLoginAsync(identity, http, config, account.Id, credentials.Email, false);

            if (config.GetValue<bool>("Identity:VerificationRequired") && account.Verified == false)
            {
                _ = await AddLoginAsync(identity, http, config, account.Id, credentials.Email, true);
                return Results.Forbid();
            }

            return await AddLoginAsync(identity, http, config, account.Id, credentials.Email, true);
        }

        private static async Task<IResult> AddLoginAsync(IdentityContext identity, HttpContext http, IConfiguration config, Guid? accountId, string email, bool successful)
        {
            await identity.Login.AddAsync(new Login { AccountId = accountId, Email = email, Successful = successful, IpAddress = http.Connection.RemoteIpAddress });
            await identity.SaveChangesAsync();
            return successful ? Results.Text(Authorization.GenerateToken(config, accountId, email), statusCode: 200) : Results.Unauthorized();
        }
    }
}
