using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostRegister
    {
        public static async Task<IResult> RegisterAsync(CredentialsModel credentials, IdentityContext identity, HttpContext http, IConfiguration config)
        {
            if (await identity.Account.AnyAsync(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == credentials.Email))
                return Results.Conflict("Email already in use");

            var account = new Account { Id = Guid.NewGuid(), Email = credentials.Email };
            var verification = new Verification { Id = Guid.NewGuid(), AccountId = account.Id, CreatedOn = DateTime.UtcNow };
            var password = new Password { AccountId = account.Id, Hash = Encryption.GenerateHash(credentials.Password) };

            await identity.Account.AddAsync(account);
            await identity.Password.AddAsync(password);
            await identity.Verification.AddAsync(verification);

            await identity.SaveChangesAsync();

            _ = await Email.SendVerificationLinkAsync(config, http, verification, credentials.Email);

            return Results.Created(account.Id.ToString(), "Account created");
        }
    }
}
