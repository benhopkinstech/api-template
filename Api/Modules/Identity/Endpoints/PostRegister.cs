using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Api.Modules.Identity.Models;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostRegister
    {
        public static async Task<IResult> RegisterAsync(CredentialsModel credentials, IIdentityService identity, IEmailService email)
        {
            if (await identity.AnyLocalAccountByEmailAsync(credentials.Email))
                return Results.Conflict();

            var account = await identity.AddLocalAccountAsync(credentials.Email);
            await identity.AddPasswordAsync(account.Id, credentials.Password);
            var verification = await identity.AddVerificationAsync(account.Id);
            await identity.SaveChangesAsync();

            await email.SendVerificationLinkAsync(verification, account.Email);

            return Results.Created(account.Id.ToString(), null);
        }
    }
}
