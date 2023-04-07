using Api.Modules.Identity.Classes;
using Api.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Identity.Endpoints
{
    public static class Delete
    {
        [Authorize]
        public static async Task<IResult> DeleteAsync(IIdentityRepository identity, HttpContext http)
        {
            var accountId = Authorization.GetAccountId(http);
            if (accountId == null)
                return Results.NotFound();

            var account = await identity.GetAccountIncludeAllByIdAsync(accountId.Value);
            if (account == null || account.PasswordAudit == null || account.AccountAudit == null || account.Login == null 
                || account.Verification == null || account.Reset == null || account.Password == null)
                return Results.NotFound();

            await identity.DeleteAll(account.PasswordAudit, account.AccountAudit, account.Login, account.Verification, account.Reset, account.Password, account);

            return Results.Ok("Deleted");
        }
    }
}
