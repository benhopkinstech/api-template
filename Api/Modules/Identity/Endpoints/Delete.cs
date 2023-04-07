using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Endpoints
{
    public static class Delete
    {
        [Authorize]
        public static async Task<IResult> DeleteAsync(IdentityContext identity, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.Account.Where(x => x.Id == accountId).Include(x => x.PasswordAudit).Include(x => x.AccountAudit).Include(x => x.Login)
                .Include(x => x.Verification).Include(x => x.Reset).Include(x => x.Password).FirstOrDefaultAsync();
            if (account == null || account.PasswordAudit == null || account.AccountAudit == null || account.Login == null 
                || account.Verification == null || account.Reset == null || account.Password == null)
                return Results.NotFound();

            identity.PasswordAudit.RemoveRange(account.PasswordAudit);
            identity.AccountAudit.RemoveRange(account.AccountAudit);
            identity.Login.RemoveRange(account.Login);
            identity.Verification.RemoveRange(account.Verification);
            identity.Reset.RemoveRange(account.Reset);
            identity.Password.Remove(account.Password);
            identity.Account.Remove(account);
            await identity.SaveChangesAsync();

            return Results.Ok("Deleted");
        }
    }
}
