using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Api.Modules.Identity.Data.Tables;
using Api.Modules.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Endpoints
{
    public static class PostResetLink
    {
        public static async Task<IResult> SendResetLinkAsync(EmailModel email, IdentityContext identity, IConfiguration config)
        {
            var account = await identity.Account.Where(x => x.ProviderId == (short)Enums.Provider.Local && x.Email == email.Email).Include(x => x.Reset).FirstOrDefaultAsync();
            if (account == null || account.Reset == null)
                return Results.NotFound();

            if (account.Reset.Count > 0)
                identity.Reset.RemoveRange(account.Reset);

            var reset = new Reset { Id = Guid.NewGuid(), AccountId = account.Id, CreatedOn = DateTime.UtcNow };
            await identity.Reset.AddAsync(reset);
            await identity.SaveChangesAsync();

            if (!await Email.SendResetLinkAsync(config, reset, account.Email))
                return Results.BadRequest("Failed to send reset link");

            return Results.Ok("Reset link sent");
        }
    }
}
